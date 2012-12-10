using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Devices
{
    public interface IMultiButtonDevice 
    {
        byte KPLButtonMask { get; set; }
    }
}
