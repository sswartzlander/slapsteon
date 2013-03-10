using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class SlapsteonEventLogEntry
    {
        public SlapsteonEventLogEntry()
        {
            Timestamp = DateTime.Now;
        }

        public SlapsteonEventLogEntry(string deviceName, string description)
        {
            this.Timestamp = DateTime.Now;
            this.Description = description;
            this.DeviceName = deviceName;
        }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string DeviceName { get; set; }

        [DataMember] 
        public string Description { get; set; }

        public SlapsteonEventLogEntry NextLogEntry { get; set; }
    }
}
