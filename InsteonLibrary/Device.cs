using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Library
{
    public class Device
    {
        public Device(string name, DeviceAddress address)
        {
            Name = name;
            Address = address;
            Status = -1;
        }

        public DeviceAddress Address { get; set; }
        public DateTime LastOn { get; set; }
        public DateTime LastOff { get; set; }
        public DateTime NextOff { get; set; }
        public decimal Status { get; set; }
        public string Name { get; set; }

        public string AddressString
        {
            get { return Address.ToString(); }

        }

        //public bool OkayToTurnOn()
        //{
        //    if (-1 == Status) return true;

        //    if (DateTime.Now.Subtract(LastOn).TotalSeconds < 60)
        //        return false;

        //    return true;
        //}
    }
}
