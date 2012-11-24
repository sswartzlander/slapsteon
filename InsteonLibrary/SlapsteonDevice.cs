using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class SlapsteonDevice
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public DateTime LastOn { get; set; }

        [DataMember]
        public DateTime LastOff { get; set; }

        [DataMember]
        public string Address { get; set; }
    }
}
