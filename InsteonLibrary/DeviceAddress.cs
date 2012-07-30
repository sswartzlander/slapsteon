using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Library
{
    public class DeviceAddress
    {
        public DeviceAddress(byte byte1, byte byte2, byte byte3)
        {
            Byte1 = byte1;
            Byte2 = byte2;
            Byte3 = byte3;
        }
        public byte Byte1 { get; set; }
        public byte Byte2 { get; set; }
        public byte Byte3 { get; set; }
    }
}
