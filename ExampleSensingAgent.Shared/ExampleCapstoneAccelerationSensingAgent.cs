﻿// Copyright 2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using Sensus;
using Sensus.Probes.Movement;
using Sensus.Probes.Location;
using Sensus.Probes;
using System.Threading;
using Sensus.Extensions;
using Sensus.Adaptation;

namespace ExampleSensingAgent
{
    /// <summary>
    /// The sensing agent for the spring 2020 capstone active sensing project
    /// </summary>
    public class ExampleCapstoneAccelerationSensingAgent : SensingAgent
    {
        #region Private Enums
        /// <summary>
        /// These are the states that the design document is built on these states don't 
        /// perfectly map to the internal states of the sensing agent so we define a map below
        /// </summary>
        private enum DesignDocumentState
        {
            ListeningWindow, ActivePredictionWindow, InactivePredictionWindow
        }
        #endregion

        #region Private Classes
        /// <summary>
        /// A helper class for calculating, storing and passing model features.
        /// These features and their values came from a design document created by the capstone class.
        /// </summary>
        private class Features
        {
            #region Public Properties
            /// <summary>
            /// The hour in 24 hour time (i.e. 0-23) at the start of the listening window
            /// </summary>
            public int Hour { get; private set; }

            /// <summary>
            /// The day of week index at the start of the listening window with Monday == 0
            /// </summary>
            public int Day { get; private set; }

            /// <summary>
            /// True if Day is Saturday or Sunday at the start of the listening window
            /// </summary>
            public bool IsWeekend { get { return Day == 5 || Day == 6; } }

            /// <summary>
            /// The summary statistics (i.e., mean, median, SD and Range) for the accelerometer data
            /// </summary>
            public Statistics Acceleration { get; private set; }
            #endregion

            #region Constructor
            public Features(DateTime listeningWindowStart, IEnumerable<ILinearAccelerationDatum> linearAccelerationData)
            {
                //design document defines day a zero-indexed beginning on Monday.
                //by default .Net zero-indexes starting on Sunday thus this transform
                Day  = ((int)listeningWindowStart.DayOfWeek + 6) % 7;

                Hour         = listeningWindowStart.Hour;
                Acceleration = new Statistics(linearAccelerationData.Select(ToMagnitude));
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// Convert the vector form of the accelerometer datums into their magnitude
            /// </summary>
            /// <param name="datum">an accelerometer datum representing a 3 dimensional vector</param>
            /// <returns>the magnitude of the given 3 dimensional acceleration datum </returns>
            private double ToMagnitude(ILinearAccelerationDatum datum)
            {
                return Math.Sqrt(Math.Pow(datum.X, 2) + Math.Pow(datum.Y, 2) + Math.Pow(datum.Z, 2));
            }
            #endregion
        }

        /// <summary>
        /// A helper class to make storing and referencing summary statistics easier
        /// </summary>
        private class Statistics
        {
            #region Public Properties
            public double Mean { get; private set; }
            public double Median { get; private set; }
            public double Range { get; private set; }
            public double Variance { get; private set; }
            public double StandardDeviation { get { return Math.Sqrt(Variance); } }
            #endregion

            #region Constructor
            public Statistics(IEnumerable<double> values)
            {
                // make sure the values are only 
                // materialized into memory once
                values = values.DefaultIfEmpty().ToArray();

                Mean = CalculateMean(values);
                Median = CalculateMedian(values);
                Range = CalculateRange(values);
                Variance = CalculateVariance(values);
            }
            #endregion

            #region Private Methods
            private double CalculateMean(IEnumerable<double> values)
            {
                return values.Average();
            }

            private double CalculateMedian(IEnumerable<double> values)
            {
                //this could be made faster
                var ys = values.OrderBy(x => x).ToList();
                double mid = (ys.Count - 1) / 2.0;
                return (ys[(int)(mid)] + ys[(int)(mid + 0.5)]) / 2;
            }

            private double CalculateVariance(IEnumerable<double> values)
            {
                var avg = values.Average();

                return Math.Sqrt(values.Select(v => Math.Pow(v - avg, 2)).Average());
            }

            private double CalculateRange(IEnumerable<double> values)
            {
                return values.Max() - values.Min();
            }
            #endregion
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// Time in which the sensors plus model determine if sensors should be on or off in prediction window
        /// </summary>
        private TimeSpan ListeningWindow
        {
            get
            {
                return ActiveObservationDuration.Value;
            }

            set
            {
                //we set this value so all criteria tests consider the same window size
                MaxObservedDataAge = value;
                
                ActiveObservationDuration = value;
                ActionInterval = value + PredictingWindow;
            }
        }

        /// <summary>
        /// Time during which the sensors are controlled by this class based on the results of the listening window and model
        /// </summary>
        private TimeSpan PredictingWindow
        {
            get
            {
                return ControlCompletionCheckInterval;
            }

            set
            {
                ControlCompletionCheckInterval = value;
                ActionInterval = ListeningWindow + value;
            }
        }

        /// <summary>
        /// Features for the listening window one cycle ago (i.e., the previous cycle)
        /// </summary>
        private Features FeaturesLag1 { get; set; }
        
        /// <summary>
        /// Features for the listening window two cycles ago
        /// </summary>
        private Features FeaturesLag2 { get; set; }
        
        /// <summary>
        /// Features for the listening window three cycles ago
        /// </summary>
        private Features FeaturesLag3 { get; set; }

        /// <summary>
        /// The prediction threshold for determining if sensors should be turned on
        /// </summary>
        private double Threshold { get; set; }

        /// <summary>
        /// The data frequency at which data is recorded when a probe is under active control
        /// </summary>
        private int ProbeHz { get; set; }
        #endregion

        #region Constructors
        public ExampleCapstoneAccelerationSensingAgent(): base("Capstone-2020", "ALM/TOD", default, default, default)
        {
            ListeningWindow  = TimeSpan.FromSeconds(10);  //provided by Mehdi in an email
            PredictingWindow = TimeSpan.FromSeconds(300); //provided by Mehdi in an email
            Threshold        = 0.5; //not provided
            ProbeHz          = 20;  //provided by Mehdi in an email
        }
        #endregion

        #region Public Override Methods
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            //Whenever a protocol first starts all the probes are on but the sensing agent is in an idle state.
            //This causes problems with turning probes on and off. To address this we tell the sensing agent that it just
            //transitioned from active (where all the probes would be on) to idle (which is the state its actually in).
            //This transition places our sensing agent into a state where the probes that are on align with the idle state. 
            await OnStateChangedAsync(SensingAgentState.ActiveControl, SensingAgentState.Idle, CancellationToken.None);

            //We then immediately start a listening window to determine if sensors should be on since we just turned them off.
            //it should be noted that the unusual assignment to an empty variable is done to hide a warning where the compiler
            //complaines about the fact that we don't call ActAsync with "await". We intentionally don't call it with await because
            //the InitializeAsync method is called as part of the protocol starting procedure and awaiting for the entire listening
            //window causes a bad user experience since appears that the protocol is hanging when in fact it is simply listening
            var task = ActAsync(CancellationToken.None);
        }
        #endregion

        #region Protected Overrides Methods
        protected override Task ProtectedSetPolicyAsync(JObject policy)
        {
            ListeningWindow  = TimeSpan.FromSeconds(double.Parse(policy["cps-listening"].ToString()));
            PredictingWindow = TimeSpan.FromSeconds(double.Parse(policy["cps-predicting"].ToString()));
            Threshold        = double.Parse(policy["cps-threshold"].ToString());
            ProbeHz          = int.Parse(policy["cps-hz"].ToString());

            return Task.CompletedTask;
        }

        protected override bool ObservedDataMeetControlCriterion(Dictionary<Type, List<IDatum>> typeData)
        {
            //we return on opportunitistc observations so that the
            //listening window which we average over will always be
            //the full length of time we desire and each saved
            //feature lag will also be for the full listening time.
            if(State == SensingAgentState.OpportunisticObservation)
            {
                return false;
            }

            //collect sensor data for making decisions
            var linearAccelerationData = GetObservedData<ILinearAccelerationDatum>().Cast<ILinearAccelerationDatum>().ToArray();
            var listeningWindowStart   = DateTime.Now.Subtract(ListeningWindow);

            //calculate and store features for model decision
            var featuresLag0 = new Features(listeningWindowStart, linearAccelerationData);
            var featuresLag1 = FeaturesLag1;
            var featuresLag2 = FeaturesLag2;
            var featuresLag3 = FeaturesLag3;

            //bump feature lags up by 1
            FeaturesLag1 = featuresLag0;
            FeaturesLag2 = featuresLag1;
            FeaturesLag3 = featuresLag2;

            //use stored features to make control decision
            return ShouldControlPredictionWindow(new[] { featuresLag0, featuresLag1, featuresLag2, featuresLag3 });
        }

        protected override async Task OnStateChangedAsync(SensingAgentState previousState, SensingAgentState currentState, CancellationToken cancellationToken)
        {
            await base.OnStateChangedAsync(previousState, currentState, cancellationToken);

            if (currentState == SensingAgentState.OpportunisticControl)
            {
                throw new Exception("Error, opportunistic control should be disabled for this agent");
            }

            var previousDesignDocumentState = ToDesignDocumentState(previousState);
            var currentDesignDocumentState  = ToDesignDocumentState(currentState);

            if (currentDesignDocumentState != previousDesignDocumentState)
            {
                var previousProbes = ToDesignDocumentProbes(previousDesignDocumentState).ToArray();
                var currentProbes  = ToDesignDocumentProbes(currentDesignDocumentState).ToArray();

                var probesToIgnore = previousProbes.Intersect(currentProbes).ToArray();
                var probesToStop   = previousProbes.Except(probesToIgnore).ToArray();
                var probesToStart  = currentProbes.Except(probesToIgnore).ToArray();

                foreach (var probe in probesToStop)
                {
                    await probe.StopAsync();
                }

                foreach(var probe in probesToStart)
                {
                    probe.MaxDataStoresPerSecond = ProbeHz;
                    await probe.StartAsync();
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This function contains the decision rule used to take control of the sensors
        /// </summary>
        /// <param name="features">the current and three previous window features for model</param>
        /// <returns>true if we should take control of the sensors otherwise false</returns>
        private bool ShouldControlPredictionWindow(Features[] features)
        {
            var logOdds     = LogisticCoefficientArray().Zip(LogisticFeatureArray(features), (coeff, feat) => coeff * feat).Sum();
            var probability = 1 / (1 + Math.Exp(-logOdds));

            return probability > Threshold;
        }

        /// <summary>
        /// I have manually entered the coeffecients calculated by Lee and published here
        /// https://docs.google.com/spreadsheets/d/1PMLEeIJmll0q17AfN-wJtE-kZQkEEPivzmbz6ZqvqbY/edit?usp=sharing
        /// </summary>
        /// <returns>the ordered enumeration of coefficients to be multiplied by features in logistic regression</returns>
        private IEnumerable<double> LogisticCoefficientArray()
        {
            yield return 5.63266503524798; //Mean
            yield return 8.04525608288699; //Median
            yield return -4.81048518038808; //StdDev
            yield return 0.8747959277907; //Range
            yield return -0.239184335512369; //Weekend
            yield return 0.38258810681683; //Mean_Lag1
            yield return 0.140387153053747; //Mean_Lag2
            yield return 0.122013362597331; //Mean_Lag3
            yield return 0.143413522190335; //Hour_0
            yield return -0.326810807022531; //Hour_1
            yield return -0.66023166841557; //Hour_2
            yield return -1.1781152937892; //Hour_3
            yield return -1.75599255231115; //Hour_4
            yield return -1.71331423402301; //Hour_5
            yield return -1.6073578712332; //Hour_6
            yield return -0.964254297086296; //Hour_7
            yield return -0.333204903480208; //Hour_8
            yield return 0.0897604682713143; //Hour_9
            yield return 0.275851343057055; //Hour_10
            yield return 0.39231229693617; //Hour_11
            yield return 0.534206303176744; //Hour_12
            yield return 0.550567177237923; //Hour_13
            yield return 0.445326590625548; //Hour_14
            yield return 0.481962539399613; //Hour_15
            yield return 0.562196539344065; //Hour_16
            yield return 0.499077852599594; //Hour_17
            yield return 0.628422242015625; //Hour_18
            yield return 0.461599460854535; //Hour_19
            yield return 0.480105654312706; //Hour_20
            yield return 0.547435185098384; //Hour_21
            yield return 0.464194755942784; //Hour_22
            yield return 0.390767671628528; //Hour_23
            yield return -0.242759499623908; //DayOfWeek_0
            yield return -0.325778210991607; //DayOfWeek_1
            yield return -0.292001735750749; //DayOfWeek_2
            yield return -0.319007943271427; //DayOfWeek_3
            yield return -0.173350299555425; //DayOfWeek_4
            yield return -0.147612811958076; //DayOfWeek_5
            yield return -0.0915715235346701; //DayOfWeek_6
            yield return -1.59208202468202; //intercept
        }

        /// <summary>
        /// Converts our array of features into the appropriate form for the logistic regression
        /// </summary>
        /// <param name="features">an array of features where the index is the lag</param>
        /// <returns>the ordered enumeration of features to be multiplied by coefficients in logistic regression</returns>
        private IEnumerable<double> LogisticFeatureArray(Features[] features)
        {
            yield return features[0].Acceleration.Mean;
            yield return features[0].Acceleration.Median;
            yield return features[0].Acceleration.StandardDeviation;
            yield return features[0].Acceleration.Range;
            yield return Convert.ToDouble(features[0].IsWeekend);
            
            for(var lag = 1; lag < 4; lag++)
            {
                yield return features[lag]?.Acceleration.Mean ?? 0;
            }
            
            for(var hour = 0; hour < 24; hour++)
            {
                yield return Convert.ToDouble(features[0].Hour == hour);
            }

            for(var day = 0; day < 7; day++)
            {
                yield return Convert.ToDouble(features[0].Day == day);
            }

            // the intercept
            yield return 1;
        }

        /// <summary>
        /// A function to map state to the constructs used/defined in the design document
        /// </summary>
        /// <param name="state">a sensing agent state</param>
        /// <returns>The state in terms of design document constructs</returns>
        private DesignDocumentState ToDesignDocumentState(SensingAgentState state)
        {
            if (state == SensingAgentState.ActiveObservation)
            {
                return DesignDocumentState.ListeningWindow;
            }
            else if(state == SensingAgentState.ActiveControl)
            {
                return DesignDocumentState.ActivePredictionWindow;
            }
            else
            {
                return DesignDocumentState.InactivePredictionWindow;
            }
        }

        /// <summary>
        /// A function to map state to probes as defined in the design document and an email from Mehdi
        /// </summary>
        /// <param name="state">a sensing agent state</param>
        /// <returns>The state in terms of design document constructs</returns>
        private IEnumerable<IListeningProbe> ToDesignDocumentProbes(DesignDocumentState state)
        {
            //it is worth noting that there is a bug in TryGetProbe if it is called with a more specific interface than IProbe
            //this is the reason why we pass in IProbe and then typecast to IListeningProbe after the probe is returned
            IProbe probe;

            if (state == DesignDocumentState.ListeningWindow)
            {
                if(Protocol.TryGetProbe<ILinearAccelerationDatum, IProbe>(out probe)) yield return (IListeningProbe)probe;
            }

            if(state == DesignDocumentState.ActivePredictionWindow)
            {
                if(Protocol.TryGetProbe<ILinearAccelerationDatum, IProbe>(out probe)) yield return (IListeningProbe)probe;
                if(Protocol.TryGetProbe<IAccelerometerDatum     , IProbe>(out probe)) yield return (IListeningProbe)probe;
                if(Protocol.TryGetProbe<IGyroscopeDatum         , IProbe>(out probe)) yield return (IListeningProbe)probe;
                if(Protocol.TryGetProbe<IAttitudeDatum          , IProbe>(out probe)) yield return (IListeningProbe)probe;
                if(Protocol.TryGetProbe<IAltitudeDatum          , IProbe>(out probe)) yield return (IListeningProbe)probe;
                if(Protocol.TryGetProbe<IMagnetometerDatum      , IProbe>(out probe)) yield return (IListeningProbe)probe;
                if(Protocol.TryGetProbe<ICompassDatum           , IProbe>(out probe)) yield return (IListeningProbe)probe;
            }
        }
        #endregion
    }
}