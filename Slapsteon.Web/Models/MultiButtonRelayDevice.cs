﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Slapsteon.Web.Models
{
    public class MultiButtonRelayDevice : RelayDevice
    {
        public byte KPLButtonMask { get; set; }

    }
}