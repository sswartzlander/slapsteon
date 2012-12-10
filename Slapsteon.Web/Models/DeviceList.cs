using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slapsteon.Web.Models
{
    public class DeviceList
    {
        public DeviceList(IEnumerable<DeviceOld> devices)
        {
            Devices = devices;
        }

        public IEnumerable<DeviceOld> Devices { get; private set; }
    }
}