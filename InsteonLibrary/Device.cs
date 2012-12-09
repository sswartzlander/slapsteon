using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    [KnownType(typeof(FanDevice))]
    [KnownType(typeof(DimmerDevice))]
    [KnownType(typeof(RelayDevice))]
    [KnownType(typeof(MultiButtonDimmerDevice))]
    [KnownType(typeof(MultiButtonRelayDevice))]
    [KnownType(typeof(PLMDevice))]
    [KnownType(typeof(SensorDevice))]
    [KnownType(typeof(IODevice))]
    [KnownType(typeof(IMultiButtonDevice))]
    public abstract class Device
    {
        private LightOffTimer _timer;

        public Device()
        {

        }

        public void SetTimer(InsteonHandler handler)
        {
            if (!DefaultOffMinutes.HasValue)
                return;

            if (null != _timer)
            {
                _timer.Reset();
                _timer = null;
            }

            _timer = new LightOffTimer(this, new TimeSpan(0, DefaultOffMinutes.Value, 0), handler);
            _timer.Start();
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
        public string Name { get; set; }

        public int? DefaultOffMinutes { get; set; }

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
    }
}
