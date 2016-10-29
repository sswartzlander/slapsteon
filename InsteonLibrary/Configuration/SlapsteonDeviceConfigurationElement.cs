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

        [ConfigurationProperty("isKPL", IsKey=false, IsRequired=false)]
        public bool? IsKPL
        {
            get { return this["isKPL"] as bool?; }
            set { this["isKPL"] = value; }
        }

        [ConfigurationProperty("isBatteryDevice", IsKey = false, IsRequired = false)]
        public bool? IsBatteryDevice
        {
            get { return this["isBatteryDevice"] as bool?; }
            set { this["isBatteryDevice"] = value; }

        }

        [ConfigurationProperty("isIODevice", IsKey = false, IsRequired = false)]
        public bool? IsIODevice
        {
            get { return this["isIODevice"] as bool?; }
            set { this["isIODevice"] = value; }
        }

        [ConfigurationProperty("isOnAtSunset", DefaultValue = false)]
        public bool? IsOnAtSunset
        {
            get { return this["isOnAtSunset"] as bool?; }
            set { this["isOnAtSunset"] = value; }
        }

        [ConfigurationProperty("isOffAtSunrise", DefaultValue = false)]
        public bool? IsOffAtSunrise
        {
            get { return this["isOffAtSunrise"] as bool?; }
            set { this["isOffAtSunrise"] = value; }
        }

        [ConfigurationProperty("floor")]
        public string Floor
        {
            get { return this["floor"] as string; }
            set { this["floor"] = value; }
        }

        [ConfigurationProperty("isThermostat", DefaultValue = false)]
        public bool? IsThermostat
        {
            get { return this["isThermostat"] as bool?; }
            set { this["isThermostat"] = value; }
        }

        [ConfigurationProperty("thermostatMode", DefaultValue="cool", IsRequired = false)]
        public string ThermostatMode
        {
            get { return this["thermostatMode"] as string; }
            set { this["thermostatMode"] = value; }
        }

        [ConfigurationProperty("thermostatSetPoint", DefaultValue = "70", IsRequired = false)]
        public string ThermostatSetPoint
        {
            get { return this["thermostatSetPoint"] as string; }
            set { this["thermostatSetPoint"] = value; }
        }

        [ConfigurationProperty("isRandomOn", DefaultValue = false, IsRequired = false)]
        public bool? IsRandomOn
        {
            get { return this["isRandomOn"] as bool?; }
            set { this["isRandomOn"] = value; }
        }

        [ConfigurationProperty("randomStartTime", DefaultValue = null, IsRequired = false)]
        public int? RandomStartTime
        {
            get { return this["randomStartTime"] as int?; }
            set { this["randomStartTime"] = value; }
        }

        [ConfigurationProperty("randomRunDuration", DefaultValue = null, IsRequired = false)]
        public int? RandomRunDuration
        {
            get { return this["randomRunDuration"] as int?; }
            set { this["randomRunDuration"] = value; }
        }

        [ConfigurationProperty("randomDurationMin", DefaultValue = null, IsRequired = false)]
        public int? RandomDurationMin
        {
            get { return this["randomDurationMin"] as int?; }
            set { this["randomDurationMin"] = value; }
        }

        [ConfigurationProperty("randomDurationMax", DefaultValue = null, IsRequired = false)]
        public int? RandomDurationMax
        {
            get { return this["randomDurationMax"] as int?; }
            set { this["randomDurationMax"] = value; }
        }

        [ConfigurationProperty("randomOnChance", DefaultValue = null, IsRequired = false)]
        public int? RandomOnChance
        {
            get { return this["randomOnChance"] as int?; }
            set { this["randomOnChance"] = value; }
        }
    }
}
