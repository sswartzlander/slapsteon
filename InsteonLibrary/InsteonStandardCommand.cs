using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insteon.Devices;

namespace Insteon.Library
{
    public class InsteonStandardMessage
    {


        private DeviceAddress _sourceAddress;
        private DeviceAddress _targetAddress;
        private byte _command1;
        private byte _command2;
        private MessageFlag _flag;
        private byte _flagByte;

        public InsteonStandardMessage(DeviceAddress sourceAddress, DeviceAddress targetAddress, byte command1, byte command2, byte flagByte)
        {
            _sourceAddress = sourceAddress;
            _targetAddress = targetAddress;
            _command1 = command1;
            _command2 = command2;
            _flag = (MessageFlag)(flagByte & ((byte)0xE0));
            _flagByte = flagByte;
        }

        public byte[] ToBytes()
        {
            return null;
        }

        public DateTime TimeSent { get; set; }
        public bool AckReceived { get; set; }
        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public string CommandDescription { get; set; }


        public string FlagDescription
        {
            get
            {
                if ((_flag & MessageFlag.ACKDirectMessage) == MessageFlag.ACKDirectMessage)
                    return "ACK Direct Message";
                else if ((_flag & MessageFlag.ACKGroupCleanupDirectMessage) == MessageFlag.ACKGroupCleanupDirectMessage)
                    return "ACK Group Cleanup Direct Message";
                else if ((_flag & MessageFlag.BroadcastMessage) == MessageFlag.BroadcastMessage)
                    return "Broadcast Message";
                else if ((_flag & MessageFlag.DirectMessage) == MessageFlag.DirectMessage)
                    return "Direct Message";
                else if ((_flag & MessageFlag.GroupBroadcastMessage) == MessageFlag.GroupBroadcastMessage)
                    return "Group Broadcast Message";
                else if ((_flag & MessageFlag.GroupCleanupDirectMessage) == MessageFlag.GroupCleanupDirectMessage)
                    return "Group Cleanup Direct Message";
                else if ((_flag & MessageFlag.NAKDirectMessage) == MessageFlag.NAKDirectMessage)
                    return "NAK Direct Message";
                else if ((_flag & MessageFlag.NAKGroupCleanupDirectMessage) == MessageFlag.NAKGroupCleanupDirectMessage)
                    return "NAK Group Cleanup Direct Message";
                else
                    return "Unknown Flag";
            }
        }

    }
}
