using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Library
{
    public enum FlagsAck
    {
        BroadcastMessage = 0x80,
        DirectMessage = 0x00,
        ACKDirectMessage = 0x20,
        NAKDirectMessage = 0xA0,
        GroupBroadcastMessage = 0xC0,
        GroupCleanupDirectMessage = 0x40,
        ACKGroupCleanupDirectMessage = 0x60,
        NAKGroupCleanupDirectMessage = 0xE0
    }
}
