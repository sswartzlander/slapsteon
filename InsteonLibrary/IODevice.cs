using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Insteon.Library
{
    [DataContract]
    public class IODevice : Device
    {
        public IODevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }

    }
}
