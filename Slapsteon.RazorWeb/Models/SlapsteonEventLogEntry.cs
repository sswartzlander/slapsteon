using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Slapsteon.RazorWeb.Models
{
    [DataContract]
    public class SlapsteonEventLogEntry
    {
        public SlapsteonEventLogEntry()
        {
        }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string DeviceName { get; set; }

        [DataMember] 
        public string Description { get; set; }

    }
}