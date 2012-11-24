using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Slapsteon.Web.Models
{
    public class Light
    {
        public Light(string name, int level)
        {
            Name = name;
            Level = level;
        }

        public int Level { get; set; }
        public string Name { get; set; }
    }

    public class Lights
    {
        public List<Light> AllLights { get; set; }
    }
}