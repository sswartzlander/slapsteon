using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class PLMDevice : Device
    {
        public PLMDevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }
    }
}
