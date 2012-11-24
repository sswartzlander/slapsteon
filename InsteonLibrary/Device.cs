using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class Device
    {
        public Device()
        {

        }

        private List<Device> _slaveDevices;
        
        public Device(string name, DeviceAddress address)
        {
            Name = name;
            Address = address;
            Status = -1;
        }

        public DeviceAddress Address { get; set; }

        [DataMember]
        public DateTime LastOn { get; set; }
        [DataMember]
        public string LastOnString
        {
            get { return LastOn.ToString(); }
            set { ;}
        }
        
        [DataMember]
        public DateTime LastOff { get; set; }
        [DataMember]
        public string LastOffString
        {
            get { return LastOff.ToString(); }
            set { ;}
        }
        public DateTime NextOff { get; set; }
        
        [DataMember]
        public decimal Status { get; set; }

        public byte Delta { get; set; }
        [DataMember]
        public string DeltaString
        {
            get { return Delta.ToString("X"); }
            set { ;}
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsPLM { get; set; }
        [DataMember]
        public bool IsDimmable { get; set; }

        private Dictionary<string,AddressRecord> _aldb;
        public Dictionary<string, AddressRecord> ALDB
        {
            get
            {
                if (null == _aldb)
                    _aldb = new Dictionary<string,AddressRecord>();
                return _aldb;
            }
            set { _aldb = value; }
        }

        [DataMember]
        public string AddressString
        {
            get { return Address.ToString(); }
            set { ;}

        }

        public List<Device> SlaveDevices
        {
            get
            {
                if (null == _slaveDevices)
                    _slaveDevices = new List<Device>();
                return _slaveDevices;
            }
            set
            {
                _slaveDevices = value;
            }
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
