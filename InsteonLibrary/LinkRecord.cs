using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insteon.Devices;

namespace Insteon.Library
{
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

        public string ReferenceDeviceName { get; set; }

        public byte RecordControl
        {
            get { return _recordControl; }
            set
            {
                _recordControl = value;
                ParseRecordControl();
            }
        }
        public DeviceAddress Address { get; set; }

        public byte Group { get; set; }

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

        public bool InUse { get { return _inUse; } }
        public LinkType Type { get { return _type; } }
        public bool HighWaterMark { get { return _highWaterMark; } }
        public int SmartHops { get { return _smartHops; } }

        // local data
        public byte OnLevel { get { return _onLevel; } }
        public byte RampRate { get { return _rampRate; } }

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
