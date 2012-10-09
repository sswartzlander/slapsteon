using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class AddressRecord
    {
        public AddressRecord()
        {

        }

        public AddressRecord(AddressEntryType type, byte groupNumber, DeviceAddress address, byte ld1, byte ld2, byte ld3)
        {
            Type = type;
            GroupNumber = groupNumber;
            Address = address;
            LocalData1 = ld1;
            LocalData3 = ld2;
            LocalData2 = ld3;
        }

        public AddressEntryType Type { get; set; }
        [DataMember]
        public byte GroupNumber { get; set; }
        public DeviceAddress Address { get; set; }

        [DataMember]
        public string AddressDeviceName { get; set; }
        public byte LocalData1 { get; set; }
        public byte LocalData2 { get; set; }
        public byte LocalData3 { get; set; }
        [DataMember]
        public string AddressOffset { get; set; }

        [DataMember]
        public string AddressEntryType
        {
            get
            {
                return Type.ToString();
            }
            set { ;}
        }

        [DataMember]
        public string AddressString
        {
            get { return Address.ToString(); }
            set { ;}
        }

        [DataMember]
        public string LocalData_1
        {
            get { return "0x" + LocalData1.ToString("X"); }
            set { ;}
        }
        [DataMember]
        public string LocalData_2
        {
            get { return "0x" + LocalData2.ToString("X"); }
            set { ;}
        }
        [DataMember]
        public string LocalData_3
        {
            get { return "0x" + LocalData3.ToString("X"); }
            set { ;}
        }
    }
}
