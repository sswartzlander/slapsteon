using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using log4net;

namespace Insteon.Devices
{
    public class LightOffTimer
    {
        private Device _device;
        private TimeSpan _duration;
        private DeviceTimerCallBack _timerCallback;
        private Timer _lightTimer;
        private static readonly ILog log = LogManager.GetLogger("Insteon");

        public LightOffTimer(Device device, TimeSpan duration, DeviceTimerCallBack timerCallback) 
        {
            _device = device;
            _duration = duration;
            _timerCallback = timerCallback;

            _lightTimer = new Timer(_duration.TotalMilliseconds);
        }

        public void Start() {

            _lightTimer.Elapsed += new ElapsedEventHandler(lightTimer_Elapsed);
            _lightTimer.Start();
            log.InfoFormat("Started countdown timer on device {0} for {1} minutes.", _device.Name, _duration.TotalMinutes);
        }

        public void Reset()
        {
            _lightTimer.Stop();
            _lightTimer.Dispose();
        }

        void lightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _lightTimer.Dispose();
        }
    }
}
