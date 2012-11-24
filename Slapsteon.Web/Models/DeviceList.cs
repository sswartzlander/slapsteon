using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slapsteon.Web.Models
{
    public class DeviceList
    {
        public DeviceList(IEnumerable<Device> devices)
        {
            Devices = devices;
        }

        public IEnumerable<Device> Devices { get; private set; }
    }
}