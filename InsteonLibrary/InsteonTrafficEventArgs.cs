using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insteon.Devices;

namespace Insteon.Library
{
    public class InsteonTrafficEventArgs : EventArgs
    {
        public Device Source { get; set; }
        public DeviceAddress Destination { get; set; }

        public FlagsAck Flags { get; set; }
        public byte Command1 { get; set; }
        public byte Command2 { get; set; }

        public string Description { get; set; }

        public string ToString()
        {
            return string.Format("Source: {0}, Dest: {1}, Flags: {2}, Command1: {3}, Command2: {4}. (Description: {5})",
                Source.Name.ToString(), Destination.ToString(), Flags, Command1.ToString("X"), Command2.ToString("X"), Description);
        }
    }
}
