using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slapsteon.Web.Models
{
    public class DeviceOld
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Status { get; set; }
        public DateTime LastOn { get; set; }
        public DateTime LastOff { get; set; }
        public bool IsFan { get; set; }
        public bool IsPLM { get; set; }
        public bool IsDimmable { get; set; }
    }
}