using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Library
{
    public class AddressRecord
    {
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
        public byte GroupNumber { get; set; }
        public DeviceAddress Address { get; set; }
        public byte LocalData1 { get; set; }
        public byte LocalData2 { get; set; }
        public byte LocalData3 { get; set; }
        public string AddressOffset { get; set; }
        
    }
}
