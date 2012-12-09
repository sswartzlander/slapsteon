using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class MultiButtonDimmerDevice : DimmerDevice, IMultiButtonDevice
    {
        public MultiButtonDimmerDevice(string deviceName, DeviceAddress deviceAddress) : base(deviceName, deviceAddress) { }
        [DataMember]
        public byte KPLButtonMask { get; set; }

    }
}
