using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Devices
{
    [DataContract]
    public class RelayDevice : Device
    {
        public RelayDevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }
    }
}
