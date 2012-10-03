using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Insteon.Library.Configuration
{
    public class SlapsteonConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("slapsteonDevices", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(SlapsteonConfigurationElementCollection<SlapsteonDeviceConfigurationElement>), AddItemName = "device")]
        public SlapsteonConfigurationElementCollection<SlapsteonDeviceConfigurationElement> SlapsteonDevices
        {
            get
            {
                return base["slapsteonDevices"] as SlapsteonConfigurationElementCollection<SlapsteonDeviceConfigurationElement>;
            }
        }
    }
}
