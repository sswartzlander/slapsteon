using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Slapsteon.Web.Models
{
    public class Device
    {
        public DateTime LastOn { get; set; }
        public DateTime LastOff { get; set; }
        public decimal Status { get; set; }
        public string Name { get; set; }
    }
}