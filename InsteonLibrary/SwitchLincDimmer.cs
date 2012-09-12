using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Insteon.Library
{
    public class SwitchLincDimmer : SwitchLinc
    {
        public SwitchLincDimmer(string name, DeviceAddress address)
            : base(name, address)
        {

        }

        public void RampOn(SerialPort plm, byte[] rampRate)
        {

        }

        public void RampOff(SerialPort plm, byte[] rampRate)
        {

        }
    }
}
