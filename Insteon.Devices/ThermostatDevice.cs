using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Devices
{
    [DataContract]
    public class ThermostatDevice : Device
    {
        public ThermostatDevice(string name, DeviceAddress address, Mode mode, int setPoint) : base(name, address)
        {
            if (mode == Mode.Cooling)
            {
                CoolSetPoint = setPoint;
                HeatSetPoint = -1;
            }
            else if (mode == Mode.Heating)
            {
                HeatSetPoint = setPoint;
                CoolSetPoint = -1;
            }
            CurrentMode = mode;

            AmbientTemperature = -1;

        } 

        [DataMember]
        public int CoolSetPoint { get; set; }
        [DataMember]
        public int HeatSetPoint { get; set; }
        [DataMember]
        public int AmbientTemperature { get; set; }
        [DataMember]
        public int Humidity { get; set; }

        [DataMember]
        public Mode CurrentMode { get; set; }

        [DataMember]
        public FanMode Fan { get; set; }

        public enum Mode
        {
            Off = 0x00,
            Auto= 0x01,
            Heating = 0x02,
            Cooling = 0x03,
            Program = 0x04
        }

        public enum FanMode
        {
            Auto = 0x00,
            AlwaysOn = 0x01
        }
    }
}
