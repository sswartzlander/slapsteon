using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Insteon.Library.Configuration
{
    public class SlapsteonDeviceConfigurationElement : SlapsteonConfigurationElement
    {
        [ConfigurationProperty("address", IsKey = true, IsRequired = true)]
        public string Address
        {
            get
            {
                return this["address"] as string;
            }
            set
            {
                this["address"] = value;
            }
        }

        [ConfigurationProperty("name", IsKey = false, IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("isDimmable", IsKey = false, IsRequired = false)]
        public bool? IsDimmable
        {
            get
            {
                return this["isDimmable"] as bool?;
            }
            set
            {
                this["isDimmable"] = value;
            }
        }

        [ConfigurationProperty("isPLM", IsKey = false, IsRequired = false)]
        public bool? IsPLM
        {
            get
            {
                return this["isPLM"] as bool?;
            }
            set
            {
                this["isPLM"] = value;
            }
        }

        [ConfigurationProperty("isFan", IsKey = false, IsRequired = false)]
        public bool? IsFan
        {
            get
            {
                return this["isFan"] as bool?;
            }
            set
            {
                this["isFan"] = value;
            }
        }

        public override string GetKey()
        {
            return Address;
        }

        [ConfigurationProperty("slaveDevices", IsKey = false, IsRequired = false)]
        public string SlaveDevices
        {
            get { return this["slaveDevices"] as string; }
            set { this["slaveDevices"] = value; }
        }

        [ConfigurationProperty("defaultOffMinutes", IsKey=false, IsRequired=false)]
        public int? DefaultOffMinutes
        {
            get { return this["defaultOffMinutes"] as int?; }
            set { this["defaultOffMinutes"] = value; }
        }

    }
}
