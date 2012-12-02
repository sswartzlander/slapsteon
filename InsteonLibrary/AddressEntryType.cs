using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Insteon.Library
{
    [DataContract]
    public enum AddressEntryType
    {
        Controller = 0xE2,
        Responder = 0xA2,
        Unknown = 0x02,
        DeletedResponder = 0x22,
        DeletedController = 0x62,
        Last = 0x00
    }
}
