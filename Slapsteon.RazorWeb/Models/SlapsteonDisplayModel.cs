using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Insteon.Devices;

namespace Slapsteon.RazorWeb.Models
{
    public class SlapsteonDisplayModel
    {
        public Device[] Devices_Floor1 { get; set; }
        public Device[] Devices_Floor2 { get; set; }
        public Device[] Devices_Basement { get; set; }
        public Device[] Devices_Exterior { get; set; }
        public SlapsteonEventLogEntry[] Events { get; set; }
    }
}