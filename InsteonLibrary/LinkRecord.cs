using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insteon.Devices;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public class LinkRecord
    {
        private byte _recordControl;
        private byte[] _localData = new byte[3];
        private bool _inUse = false;
        private LinkType _type = LinkType.Unknown;
        private bool _highWaterMark = false;
        private int _smartHops = 0;

        private byte _onLevel;
        private byte _rampRate;
        public LinkRecord()
        {

        }

        public LinkRecord(byte recordControl, byte group, DeviceAddress address, byte localData1, byte localData2, byte localData3)
        {
            Group = group;
            _recordControl = recordControl;
            Address = address;

            _localData[0] = localData1;
            _localData[1] = localData2;
            _localData[2] = localData3;

            ParseRecordControl();
            ParseLocalData();

        }
        [DataMember]
        public string ReferenceDeviceName { get; set; }

        [DataMember]
        public byte RecordControl
        {
            get { return _recordControl; }
            set
            {
                _recordControl = value;
                ParseRecordControl();
            }
        }
        [DataMember]
        public DeviceAddress Address { get; set; }

        [DataMember]
        public byte Group { get; set; }

        [DataMember]
        public byte[] LocalData
        {
            get { return _localData; }
            set
            {
                int i = 0;
                foreach (byte b in value)
                {
                    if (i > 2)
                        break;

                    _localData[i] = b;

                    i++;
                }
                ParseLocalData();
            }

        }

        // record control info

        [DataMember]
        public bool InUse { get { return _inUse; } private set { _inUse = value; } }
        [DataMember]
        public LinkType Type { get { return _type; } private set { _type = value; } }
        [DataMember]
        public bool HighWaterMark { get { return _highWaterMark; } private set { _highWaterMark = value; } }
        [DataMember]
        public int SmartHops { get { return _smartHops; } private set { _smartHops = value; } }

        // local data
        [DataMember]
        public byte OnLevel { get { return _onLevel; } private set { _onLevel = value; } }
        [DataMember]
        public byte RampRate { get { return _rampRate; } private set { _rampRate = value; } }

        private void ParseRecordControl()
        {
            _inUse = (_recordControl & 0x80) == 0x80;
            _highWaterMark = (_recordControl & 0x02) == 0x02;
            _smartHops = ((_recordControl & 18) >> 3);

            if ((_recordControl & 0x40) == 0x40) _type = LinkType.Controller;
            else _type = LinkType.Responder;
        }

        private void ParseLocalData()
        {
            _onLevel = _localData[0];
            _rampRate = _localData[1];
        }

        public enum LinkType
        {
            Controller,
            Responder,
            Unknown
        }
    }

}
