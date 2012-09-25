using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Library
{
    public enum AddressEntryType
    {
        Controller = 0xE2,
        Responder = 0xA2,
        Deleted = 0x02,
        Deleted2 = 0x22,
        Last = 0x00
    }
}
