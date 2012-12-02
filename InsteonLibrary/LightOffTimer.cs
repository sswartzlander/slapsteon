using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Insteon.Library
{
    public class LightOffTimer
    {
        private Device _device;
        private TimeSpan _duration;
        private InsteonHandler _handler;
        private Timer _lightTimer;

        public LightOffTimer(Device device, TimeSpan duration, InsteonHandler handler) 
        {
            _device = device;
            _duration = duration;
            _handler = handler;

            _lightTimer = new Timer(_duration.TotalMilliseconds);
        }

        public void Start() {

            _lightTimer.Elapsed += new ElapsedEventHandler(lightTimer_Elapsed);
            _lightTimer.Start();
        }

        public void Reset()
        {
            _lightTimer.Stop();
            _lightTimer.Dispose();
        }

        void lightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _handler.SendStandardCommand(_device.Address, Constants.STD_COMMAND_FAST_OFF, 0x00, 0x0F);
            _lightTimer.Dispose();
        }
    }
}
