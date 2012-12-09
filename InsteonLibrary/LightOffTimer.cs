﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using log4net;

namespace Insteon.Library
{
    public class LightOffTimer
    {
        private Device _device;
        private TimeSpan _duration;
        private InsteonHandler _handler;
        private Timer _lightTimer;
        private static readonly ILog log = LogManager.GetLogger("Insteon");

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
            log.InfoFormat("Started countdown timer on device {0} for {1} minutes.", _device.Name, _duration.TotalMinutes);
        }

        public void Reset()
        {
            _lightTimer.Stop();
            _lightTimer.Dispose();
        }

        void lightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _handler.SendStandardCommand(_device.Address, Constants.STD_COMMAND_FAST_OFF, 0x00, 0x0F);
            _handler.ProcessSendingRelatedEvents(Constants.STD_COMMAND_FAST_OFF, _device);
            _lightTimer.Dispose();
        }
    }
}
