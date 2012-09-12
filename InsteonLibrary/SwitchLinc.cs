using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Insteon.Library
{
    public class SwitchLinc : Device
    {
        public SwitchLinc(string name, DeviceAddress address)
            : base(name, address)
        {

        }

        public void TurnOn(SerialPort plm)
        {

        }

        public void TurnOff(SerialPort plm)
        {

        }
    }
}
