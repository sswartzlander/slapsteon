using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    class FanDevice : DimmerDevice
    {
        public FanDevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }
    }
}
