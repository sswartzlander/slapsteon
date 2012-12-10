using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Devices
{
    [DataContract]
    public class DimmerDevice : Device
    {
        public DimmerDevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }

    }
}
