using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class MultiButtonRelayDevice : RelayDevice, IMultiButtonDevice
    {
        public MultiButtonRelayDevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }
        [DataMember]
        public byte KPLButtonMask { get; set; }

    }
}
