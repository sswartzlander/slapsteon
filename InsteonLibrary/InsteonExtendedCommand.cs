using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insteon.Devices;

namespace Insteon.Library
{
    public class InsteonExtendedMessage : InsteonStandardMessage
    {
        private byte[] _data;


        public InsteonExtendedMessage(DeviceAddress sourceAddress, DeviceAddress targetAddress, byte command1, byte command2, byte[] data, byte flag)
            : base(sourceAddress, targetAddress, command1, command2, flag)
        {
            _data = data;
        }



    }
}
