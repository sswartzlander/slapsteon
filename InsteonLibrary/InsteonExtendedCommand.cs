using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insteon.Devices;

namespace Insteon.Library
{
    public class InsteonExtendedCommand : InsteonStandardCommand
    {
        public InsteonExtendedCommand(DeviceAddress targetAddress, byte command1, byte command2)
            : base(targetAddress, command1, command2)
        {

        }



    }
}
