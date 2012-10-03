using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Insteon.Library.Configuration
{
    public abstract class SlapsteonConfigurationElement : ConfigurationElement
    {
        public abstract string GetKey();
    }
}
