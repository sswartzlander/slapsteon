using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AirQualityMonitoring
{
    public class MainWorker
    {
        private bool _initialized = false;
        private List<AirQualityMonitorThread> _airQualityThreads = new List<AirQualityMonitorThread>();
//        private Thread _mainThread = null;
//        private bool _isRunning = false;
        
        public void Initialize(Dictionary<string, string> sensorDictionary)
        {
            foreach (string key in sensorDictionary.Keys)
            {
                AirQualityMonitorThread aqThread = new AirQualityMonitorThread(key, sensorDictionary[key]);
                _airQualityThreads.Add(aqThread);
            }
            
            _initialized = true;
        }

        public void Start()
        {
            if (!_initialized || _airQualityThreads.Count == 0)
                return;
            // start the monitor threads, they will do the rest..
            foreach (AirQualityMonitorThread thread in _airQualityThreads)
            {
                thread.Start();
            }
        }

        public void Stop()
        {
            foreach (AirQualityMonitorThread thread in _airQualityThreads)
            {
                thread.Stop();
            }
        }
    }
}
