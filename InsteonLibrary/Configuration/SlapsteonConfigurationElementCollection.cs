using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Insteon.Library.Configuration
{
    public  class SlapsteonConfigurationElementCollection<T> : ConfigurationElementCollection where T : SlapsteonConfigurationElement, new() 
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            SlapsteonConfigurationElement slapsteonConfigurationElement =
                element as SlapsteonConfigurationElement;
            return slapsteonConfigurationElement.GetKey();
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return base.Properties;
            }
        }

        public new T this[string key]
        {
            get
            {
                return (T)BaseGet(key);
            }
            set
            {
                if (null != BaseGet(key))
                    BaseRemove(key);
                BaseAdd(value);
            }
        }

        public T this[int index]
        {
            get
            {
                return BaseGet(index) as T;
            }
            set
            {
                if (null != BaseGet(index))
                    BaseRemoveAt(index);
                BaseAdd(value);
            }
        }

        public void Add(T configurationElement)
        {
            BaseAdd(configurationElement);
        }
    }
}
