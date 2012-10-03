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
        public byte Delta { get; set; }
        public string Name { get; set; }
        public bool IsPLM { get; set; }
        public bool IsDimmable { get; set; }

        private Dictionary<string,AddressRecord> _aldb;
        public Dictionary<string,AddressRecord> ALDB
        {
            get
            {
                if (null == _aldb)
                    _aldb = new Dictionary<string,AddressRecord>();
                return _aldb;
            }
            set { _aldb = value; }
        }

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
