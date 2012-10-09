using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class DeviceAddress
    {
        public DeviceAddress()
        {

        }

        public DeviceAddress(byte byte1, byte byte2, byte byte3)
        {
            Byte1 = byte1;
            Byte2 = byte2;
            Byte3 = byte3;
        }

        public byte Byte1 { get; set; }
        public byte Byte2 { get; set; }
        public byte Byte3 { get; set; }


        public string ToString()
        {
            return Byte1.ToString("X") + Byte2.ToString("X") + Byte3.ToString("X");
        }

        [DataMember]
        public string StringAddress
        {
            get { return this.ToString(); }
            set { ;}
        }
    }
}
