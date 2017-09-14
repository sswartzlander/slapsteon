using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AirQualityMonitoring
{
    [DataContract]
    public class AirQualityStats
    {
        public AirQualityStats()
        {
            Timestamp = DateTime.Now;
        }

        [DataMember(Name = "pm01ConcentrationCF1")]
        public int PM01ConcentrationCF1 { get; set; }

        [DataMember(Name = "pm5ConcentrationCF1")]
        public int PM25ConcentrationCF1 { get; set; }

        [DataMember(Name = "pm10ConcentrationCF1")]
        public int PM10ConcentrationCF1 { get; set; }

        [DataMember(Name = "pm01ConcentrationAtm")]
        public int PM01ConcentrationAtm { get; set; }

        [DataMember(Name = "pm5ConcentrationAtm")]
        public int PM25ConcentrationAtm { get; set; }

        [DataMember(Name = "pm10ConcentrationAtm")]
        public int PM10ConcentrationAtm { get; set; }

        [DataMember(Name = "point3MicronCount")]
        public int Point3MicronCount { get; set; }

        [DataMember(Name = "point5MicronCount")]
        public int Point5MicronCount { get; set; }

        [DataMember(Name = "oneMicronCount")]
        public int OneMicronCount { get; set; }

        [DataMember(Name = "twoPointFiveMicronCount")]
        public int TwoPointFiveMicronCount { get; set; }

        [DataMember(Name = "fiveMicronCount")]
        public int FiveMicronCount { get; set; }

        [DataMember(Name = "tenMicronCount")]
        public int TenMicronCount { get; set; }

        public DateTime Timestamp { get; private set; }

        [OnDeserialized]
        private void SetTimestamp(StreamingContext streamingContext)
        {
            Timestamp = DateTime.Now;
        }
    }
}
