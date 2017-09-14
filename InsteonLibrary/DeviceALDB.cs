using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Insteon.Library
{
    [Serializable]
    public class DeviceALDB
    {
        private List<ALDBRecord> _aldbRecords;

        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string DeviceAddress { get; set; }
        [XmlAttribute]
        public byte Delta { get; set; }
        [XmlElement]
        public List<ALDBRecord> ALDBRecords
        {
            get
            {
                if (null == _aldbRecords)
                    _aldbRecords = new List<ALDBRecord>();

                return _aldbRecords;
            }
            set
            {
                _aldbRecords = value;
            }
        }

        [XmlElement]
        public DateTime LastSync
        {
            get;
            set;
        }
    }

    [Serializable]
    public class ALDBRecord
    {
        [XmlAttribute]
        public byte AddressMSB { get; set; }
        [XmlAttribute]
        public byte AddressLSB { get; set; }
        [XmlAttribute]
        public byte Flags { get; set; }
        [XmlAttribute]
        public byte Group { get; set; }
        [XmlAttribute]
        public byte Address1 { get; set; }
        [XmlAttribute]
        public byte Address2 { get; set; }
        [XmlAttribute]
        public byte Address3 { get; set; }
        [XmlAttribute]
        public byte LocalData1 { get; set; }
        [XmlAttribute]
        public byte LocalData2 { get; set; }
        [XmlAttribute]
        public byte LocalData3 { get; set; }

        public string AddressToString()
        {
            return Address1.ToString("X").PadLeft(2, '0') + Address2.ToString("X").PadLeft(2, '0') + Address3.ToString("X").PadLeft(2, '0'); 
        }

        public string AddressIndex
        {
            get
            {
                return AddressMSB.ToString("X").PadLeft(2, '0') + AddressLSB.ToString("X").PadLeft(2, '0');
            }
        }
    }
}
