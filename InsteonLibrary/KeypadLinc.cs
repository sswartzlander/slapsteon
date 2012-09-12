using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Insteon.Library
{
    public class KeypadLinc : Device
    {
        public KeypadLinc(string name, DeviceAddress address)
            : base(name, address)
        {

        }

        public void GetStatus(SerialPort plm)
        {

        }
    }
}
