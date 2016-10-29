using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Devices
{
    public delegate void DeviceTimerCallBack(Device device);

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
    [KnownType(typeof(ThermostatDevice))]
    public class Device
    {
        private LightOffTimer _timer;

        public Device()
        {

        }

        public void SetTimer(DeviceTimerCallBack timerCallback)
        {
            if (!DefaultOffMinutes.HasValue)
                return;

            if (null != _timer)
            {
                _timer.Reset();
                _timer = null;
            }

            _timer = new LightOffTimer(this, new TimeSpan(0, DefaultOffMinutes.Value, 0), timerCallback);
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
            get { return Address != null ? Address.ToString() : null; }
            set { ;}

        }

        [DataMember]
        public string Floor
        {
            get;
            set;
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

        public bool IsSlaveDevice { get; set; }

        public bool IsOnAtSunset { get; set; }
        public bool IsOffAtSunrise { get; set; }

        public bool IsRandomOn { get; set; }
        public int? RandomOnStart { get; set; }
        public int? RandomRunDuration { get; set; }
        public int? RandomDurationMin { get; set; }
        public int? RandomDurationMax { get; set; }
        public int? RandomOnChance { get; set; }
        public bool LastOnWasRandom { get; set; }
        public DateTime? LastRandomOnTime { get; set; }
        public int? LastRandomOnLength { get; set; }
    }
}
