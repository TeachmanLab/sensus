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

using Newtonsoft.Json;
using SensusService.Probes;
using System;

namespace SensusService
{
    /// <summary>
    /// A single unit of sensed information returned by a probe.
    /// </summary>
    public abstract class Datum
    {
        /// <summary>
        /// Settings for serializing Datum objects
        /// </summary>
        private static readonly JsonSerializerSettings _serializationSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            TypeNameHandling = TypeNameHandling.All,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public static Datum FromJSON(string json)
        {
            Datum datum = null;

            try { datum = JsonConvert.DeserializeObject<Datum>(json, _serializationSettings); }
            catch (Exception ex) { SensusServiceHelper.Get().Logger.Log("Failed to convert JSON to datum:  " + ex.Message, LoggingLevel.Normal, null); }

            return datum;
        }

        private string _id;
        private string _deviceId;
        private string _probeType;
        private DateTimeOffset _timestamp;
        private int _hashCode;

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                _hashCode = _id.GetHashCode();
            }
        }

        public string DeviceId
        {
            get { return _deviceId; }
            set { _deviceId = value; }
        }

        public string ProbeType
        {
            get { return _probeType; }
            set { _probeType = value; }
        }

        public DateTimeOffset Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        [JsonIgnore]
        public string JSON
        {
            get { return JsonConvert.SerializeObject(this, Formatting.None, _serializationSettings).Replace('\n', ' ').Replace('\r', ' '); }
        }

        [JsonIgnore]
        public abstract string DisplayDetail { get; }

        private Datum() { }  // for JSON.NET deserialization

        public Datum(Probe probe, DateTimeOffset timestamp)
        {
            _probeType = probe == null ? "" : probe.GetType().FullName;  // not all data are generated by probes (e.g., reports)
            _timestamp = timestamp;
            _deviceId = SensusServiceHelper.Get().DeviceId;

            Id = Guid.NewGuid().ToString();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Datum && (obj as Datum)._id == _id;
        }

        public override string ToString()
        {
            return "Type:  " + GetType().Name + Environment.NewLine +
                   "Device ID:  " + _deviceId + Environment.NewLine + 
                   "Probe:  " + _probeType + Environment.NewLine +
                   "Timestamp:  " + _timestamp;
        }
    }
}