using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insteon.Devices;

namespace Insteon.Library
{
    public class InsteonStandardCommand
    {

        

        private DeviceAddress _targetAddress;
        private byte _command1;
        private byte _command2;

        public InsteonStandardCommand(DeviceAddress targetAddress, byte command1, byte command2)
        {
            _targetAddress = targetAddress;
            _command1 = command1;
            _command2 = command2;
        }

        public byte[] ToBytes()
        {
            return null;
        }

        public DateTime TimeSent { get; set; }
        public bool AckReceived { get; set; }

    }
}
