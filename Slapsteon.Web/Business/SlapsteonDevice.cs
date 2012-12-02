using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Slapsteon.Web.Business
{
    //[JsonObjectAttribute]
    public class SlapsteonDevice
    {
        public string Address { get; set; }
        //public Dictionary<string, AddressRecord> ALDB { get; set; }
        //decimal public bool IsPLM { get; set; }
        //public bool IsDimmable { get; set; }
        public string Name { get; set; }
        //public string DeltaString { get; set; }
        public DateTime LastOn { get; set; }
        public DateTime LastOff { get; set; }
        //public string LastOn { get; set; }
        //public string LastOff { get; set; }
        public string Status { get; set; }
        public bool IsPLM { get; set; }
        public bool IsDimmable { get; set; }
        public bool IsFan { get; set; }
    }
}