using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Library
{
    // no data contract, no need to seralize
    public class PLMDevice : Device
    {
        public PLMDevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }
    }
}
