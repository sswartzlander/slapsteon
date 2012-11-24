using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slapsteon.Web.Business
{
    public class AddressRecord
    {
        public byte GroupName { get; set; }
        public string AddressOffset { get; set; }
        public AddressEntryType Type { get; set; }
    }
}