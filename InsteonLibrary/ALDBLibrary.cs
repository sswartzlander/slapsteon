using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Insteon.Library
{
    [Serializable]
    public class ALDBLibrary
    {
        private List<DeviceALDB> _deviceALDBList;

        [XmlElement]
        public List<DeviceALDB> Devices
        {
            get
            {
                if (null == _deviceALDBList)
                    _deviceALDBList = new List<DeviceALDB>();
                return _deviceALDBList;
            }
            set
            {
                _deviceALDBList = value;
            }
        }
    }
}
