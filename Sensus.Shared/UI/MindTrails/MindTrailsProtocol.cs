// Copyright 2014 The Rector & Visitors of the University of Virginia
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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sensus.Adaptation;
using Sensus.Authentication;
using Sensus.DataStores.Local;
using Sensus.DataStores.Remote;
using Sensus.Extensions;
using Sensus.Probes;
using Sensus.Probes.User.Scripts;
using Sensus.UI.Inputs;
using Sensus.UI.MindTrailsBehind;
using Sensus.UI.UiProperties;

namespace Sensus.UI.MindTrails
{
    public class MindTrailsProtocol
    {
        public const string MANAGED_URL_STRING = "managed"; // server end point that returns json + participant ID
        private string _id;
        private string _name;
        private string _title;
        private string _participantId;
        private DateTimeOffset _randomTimeAnchor;
        private List<Probe> _probes;
        private LocalDataStore _localDataStore;
        private RemoteDataStore _remoteDataStore;
        private string _storageDirectory;
        public static MindTrailsProtocol protocol; 

        public List<SessionModel> Session { get; set; }

        public MindTrailsProtocol(string name)
        {
            _name = name;
        }

        public static async Task<MindTrailsProtocol> CreateAsync(string name)
        {
            MindTrailsProtocol protocol = new MindTrailsProtocol(name);

            await protocol.ResetAsync(true);

            //foreach (Probe probe in Probe.GetAll())
            //{
            //    protocol.AddProbe(probe);
            //}

            //SensusServiceHelper.Get().RegisterProtocol(protocol);

            return protocol;
        }
        // cannot work with any of the probes yet : 

        //private void AddProbe(Probe probe)
        //{
        //    probe.MindTrailsProtocol = this;

        //    // since the new probe was just bound to this protocol, we need to let this protocol know about this probe's default anonymization preferences.
        //    foreach (PropertyInfo anonymizableProperty in probe.DatumType.GetProperties().Where(property => property.GetCustomAttribute<Anonymizable>() != null))
        //    {
        //        Anonymizable anonymizableAttribute = anonymizableProperty.GetCustomAttribute<Anonymizable>(true);
        //        _jsonAnonymizer.SetAnonymizer(anonymizableProperty, anonymizableAttribute.DefaultAnonymizer);
        //    }

        //    _probes.Add(probe);
        //    _probes.Sort(new Comparison<Probe>((p1, p2) => p1.DisplayName.CompareTo(p2.DisplayName)));
        //}

        private async Task ResetAsync(bool resetId)
        {
            Random random = new Random();

            // reset id and storage directory (directory might exist if deserializing the same protocol multiple times)
            if (resetId)
            {
                _id = Guid.NewGuid().ToString();

                // if this is a new study (indicated by resetting the ID), randomly initialize GPS longitude offset.
                //_gpsLongitudeAnonymizationStudyOffset = LongitudeOffsetGpsAnonymizer.GetOffset(random);
            }

            // nobody else should receive the participant ID or participant anonymization offset
            _participantId = null;
            //_gpsLongitudeAnonymizationParticipantOffset = 0;

            // reset local storage
            ResetStorageDirectory();

            // pick a random time anchor within the first 1000 years AD. we got a strange exception in insights about the resulting datetime having a year
            // outside of [0,10000]. no clue how this could happen, but we'll guard against it all the same. we do this regardless of whether we're 
            // resetting the protocol ID, as everyone should have a different anchor. in the future, perhaps we'll do something similar to what we do for GPS.
            try
            {
                _randomTimeAnchor = new DateTimeOffset((long)(random.NextDouble() * new DateTimeOffset(1000, 1, 1, 0, 0, 0, new TimeSpan()).Ticks), new TimeSpan());
            }
            catch (Exception) { }

            // reset probes
            foreach (Probe probe in _probes)
            {
                await probe.ResetAsync();

                // reset enabled status of probes to the original values. probes can be disabled when the protocol is started (e.g., if the user denies health kit).
                probe.Enabled = probe.OriginallyEnabled;

                // if we reset the protocol id, assign new group and input ids to all scripts
                if (probe is ScriptProbe && resetId)
                {
                    foreach (ScriptRunner runner in (probe as ScriptProbe).ScriptRunners)
                    {
                        foreach (InputGroup inputGroup in runner.Script.InputGroups)
                        {
                            inputGroup.Id = Guid.NewGuid().ToString();

                            foreach (Input input in inputGroup.Inputs)
                            {
                                input.GroupId = inputGroup.Id;
                                input.Id = Guid.NewGuid().ToString();
                            }
                        }
                    }
                }
            }

            if (_localDataStore != null)
            {
                _localDataStore.Reset();
            }

            if (_remoteDataStore != null)
            {
                _remoteDataStore.Reset();
            }

            // reset taggings, as they're not relevant to the future of the protocol.
            TaggedEventId = null;
            TaggedEventTags.Clear();
            TaggingStartTimestamp = null;
            TaggingEndTimestamp = null;
            TaggingsToExport.Clear();

            // do not retain the authentication service. we do not want it to be passed around.
            AuthenticationService = null;
        }

        private void ResetStorageDirectory()
        {
            StorageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), _id);
        }

        public string StorageDirectory
        {
            get
            {
                try
                {
                    // test storage directory to ensure that it's valid
                    if (!Directory.Exists(_storageDirectory) || Directory.GetFiles(_storageDirectory).Length == -1)
                    {
                        throw new Exception("Invalid protocol storage directory.");
                    }
                }
                catch (Exception)
                {
                    // the storage directory is not valid. try resetting the storage directory.
                    try
                    {
                        ResetStorageDirectory();

                        if (!Directory.Exists(_storageDirectory) || Directory.GetFiles(_storageDirectory).Length == -1)
                        {
                            throw new Exception("Failed to reset protocol storage directory.");
                        }
                    }
                    catch (Exception ex)
                    {
                        SensusServiceHelper.Get().Logger.Log(ex.Message, LoggingLevel.Normal, GetType());
                        throw ex;
                    }
                }

                return _storageDirectory;
            }
            set
            {
                _storageDirectory = value;

                if (!string.IsNullOrWhiteSpace(_storageDirectory) && !Directory.Exists(_storageDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(_storageDirectory);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        /// <summary>
        /// The current event identifier for tagging. See [this article](xref:tagging_mode) for more information.
        /// </summary>
        /// <value>The tag identifier.</value>
        public string TaggedEventId { get; set; }

        /// <summary>
        /// The current tags applied during event tagging. See [this article](xref:tagging_mode) for more information.
        /// </summary>
        /// <value>The set tags.</value>
        public List<string> TaggedEventTags { get; set; } = new List<string>();
        /// <summary>
        /// The time at which the current tagging started.
        /// </summary>
        /// <value>The tagging start timestamp.</value>
        public DateTimeOffset? TaggingStartTimestamp { get; set; }
        /// <summary>
        /// The time at which the current tagging ended.
        /// </summary>
        /// <value>The tagging end timestamp.</value>
        public DateTimeOffset? TaggingEndTimestamp { get; set; }
        /// <summary>
        /// A list of taggings to export.
        /// </summary>
        /// <value>The taggings to export.</value>
        public List<string> TaggingsToExport { get; } = new List<string>();
        /// <summary>
        /// The authentication service. This is serialized to JSON; however, the only thing that is retained in the
        /// serialized JSON is the service base URL. No account or credential information is serialized; rather, 
        /// this information is refreshed when needed.
        /// </summary>
        /// <value>The management service.</value>
        public AuthenticationService AuthenticationService { get; set; }


        public static async Task<MindTrailsProtocol> DeserializeAsync(Uri uri, bool offerToReplaceExistingProtocol) // bool offerToReplaceExistingProtocol, AmazonS3Credentials credentials = null
        {
            Console.WriteLine("VERIFY it gets to deserialize"); // getting here!
            MindTrailsProtocol protocol = null;

            byte[] protocolBytes = null;

            //// check if the URI points to an S3 bucket
            //if (AmazonS3Uri.IsAmazonS3Endpoint(uri))
            //{
            //    AmazonS3Client s3Client = null;

            //    // use app-level S3 authentication if we don't have an authentication service
            //    if (credentials == null)
            //    {
            //        if (SensusContext.Current.IamAccessKey == null ||
            //            SensusContext.Current.IamAccessKeySecret == null |
            //            SensusContext.Current.IamRegion == null)
            //        {
            //            throw new Exception("You must first authenticate.");
            //        }
            //        else
            //        {
            //            s3Client = new AmazonS3Client(SensusContext.Current.IamAccessKey, SensusContext.Current.IamAccessKeySecret, RegionEndpoint.GetBySystemName(SensusContext.Current.IamRegion));
            //        }
            //    }
            //    // use authentication service S3 credentials
            //    else
            //    {
            //        s3Client = new AmazonS3Client(credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken, credentials.RegionEndpoint);
            //    }

            //    AmazonS3Uri s3URI = new AmazonS3Uri(uri);

            //    GetObjectResponse response = await s3Client.GetObjectAsync(s3URI.Bucket, s3URI.Key);

            //    if (response.HttpStatusCode == HttpStatusCode.OK)
            //    {
            //        MemoryStream byteStream = new MemoryStream();
            //        response.ResponseStream.CopyTo(byteStream);
            //        protocolBytes = byteStream.ToArray();
            //    }
            //}
            //// if we don't have an S3 URI, then download protocol bytes directly from web and deserialize.
            //else
            //{

            protocolBytes = await uri.DownloadBytesAsync(); // ONLY this line, comment out s3 portion, add to new protocol class

            Console.WriteLine("bytes downloaded");
            Console.WriteLine(protocolBytes == null); // bytes are not false 

            //}

            protocol = await DeserializeAsync(protocolBytes, offerToReplaceExistingProtocol); // look for method that deserializes from bytes
            // new method would deserialize from bytes
            Console.WriteLine("VERIFY 2");
            Console.WriteLine(protocol);
            return protocol;
        }

        // protocol is currrently null 
        public static async Task<MindTrailsProtocol> DeserializeAsync(byte[] bytes, bool offerToReplaceExistingProtocol) // method from 151
        {
            string json;
            Console.WriteLine("checking bytes");
            Console.WriteLine(bytes == null); // bytes are not false 
            //json = Encoding.Unicode.GetString(bytes);

            //string json;
            try
            {
                json = Encoding.UTF8.GetString(bytes); // instead of Unicode
                Console.WriteLine("encoded json");
                Console.WriteLine(json == null);
                Console.WriteLine(json);

            }


            catch (Exception ex)
            {
                Console.WriteLine("failed to get json string from bytes");
                throw new Exception("Failed to get json string from bytes:  " + ex.Message);
            }

            // make any necessary platform conversions to type names to allow the JSON to deserialize
            //try
            //{
            //    json = SensusServiceHelper.Get().ConvertJsonForCrossPlatform(json);
            //    Console.WriteLine("json converted");
            //    Console.WriteLine(json == null); // not null 
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("Failed to convert study:  " + ex.Message);

            //}

            protocol = new MindTrailsProtocol("placeholder");
            //Root data;
            try
            {
                // from StringExtensions.cs
                protocol.Session = json.DeserializeJson<List<SessionModel>>(); // problem // MindTrailsProtocol
                //protocol.Session = JsonConvert.DeserializeObject<List<SessionModel>>(json); // problem // MindTrailsProtocol

                Console.WriteLine("protocol deserialized");

                //Console.WriteLine(protocol.ToString());
                //data = JsonConvert.DeserializeObject<Root>(json);
            }

            catch (Exception ex)
            {
                Console.WriteLine("failed to deserialize json");
                throw new Exception("Failed to deserialize json:  " + ex.Message);
            }

            // print a protocol field to the console
            Console.WriteLine("VERIFICATION: "); // getting to here  
            //Console.WriteLine(data.firstSession[0].title);
            Console.WriteLine(protocol == null); // right now protocol is null
            // protocol is not null! 
            return protocol; 
        }

        //public Root getData(byte[] bytes)
        //{

        //}

        public string ParticipantId
        {
            get
            {
                return _participantId;
            }
            set
            {
                _participantId = value;
            }
        }
        [EntryStringUiProperty(null, false, 0, true)]
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }
    }
}