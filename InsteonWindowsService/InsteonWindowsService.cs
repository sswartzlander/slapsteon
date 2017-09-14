using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.ServiceModel.Web;
using Insteon.Library;
using System.IO.Ports;
using System.Configuration;
using System.Runtime.InteropServices;
using log4net;
using Insteon.Devices;
using System.IO;
using AirQualityMonitoring;

namespace Insteon.WindowsService
{
    public partial class InsteonWindowsService : ServiceBase
    {
        [DllImport("advapi32.dll", EntryPoint = "SetServiceStatus")]
        private static extern bool SetServiceStatus(IntPtr hServiceStatus, ref SERVICE_STATUS lpServiceStatus);

        private static string _serialPort = "COM4";
        private static SerialPort _plm;
        public static SerialPort PLM { get { return _plm; } }
        private bool _stop = false;
        private Process _iisProcess = null;
        private Thread _wcfHostThread;
        private Thread _iisExpressThread;
        private SERVICE_STATUS _serviceStatus;
        private static readonly ILog log = LogManager.GetLogger("Insteon");
        private List<Device> _randomDevices = new List<Device>();

        private DateTime? _lastSunrise = null;
        private DateTime? _lastSunset = null;

        private InsteonHandler _handler;

        private double _latitude;
        private double _longitude;

        private InsteonWebService _insteonWebService;
        private Random _random;

        private MainWorker _airQualityWorker;
        public InsteonWindowsService()
        {
            log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();

            _random = new Random();
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Starting Insteon Service.");
            try
            {
                string serialPort = ConfigurationManager.AppSettings["SerialPort"];
                if (!string.IsNullOrEmpty(serialPort))
                    _serialPort = serialPort;

                string latString = ConfigurationManager.AppSettings["Latitude"];
                if (!double.TryParse(latString, out _latitude))
                    _latitude =35.773;

                string longitudeString = ConfigurationManager.AppSettings["Longitude"];
                if (!double.TryParse(longitudeString, out _longitude))
                    _longitude = -78.888893;

                SetServiceState(State.SERVICE_START_PENDING);

                _handler = new InsteonHandler(serialPort, true);

                _handler.InsteonTrafficDetected += new InsteonHandler.InsteonTrafficHandler(_handler_InsteonTrafficDetected);
                _handler.PartyDetected += new InsteonHandler.PartyHandler(_handler_PartyDetected);

                //log.InfoFormat("Setting up random devices, handler has {0} devices", _handler.AllDevices.Count);

                //if (_handler.AllDevices.ContainsKey("22AEB1")) // entry
                //    _randomDevices.Add(_handler.AllDevices["22AEB1"]);
                //if (_handler.AllDevices.ContainsKey("1FB523")) // stairs
                //    _randomDevices.Add(_handler.AllDevices["1FB523"]);
                //if (_handler.AllDevices.ContainsKey("192A4D")) // kitchen multi
                //    _randomDevices.Add(_handler.AllDevices["192A4D"]);
                //if (_handler.AllDevices.ContainsKey("1BBECC")) // living room
                //    _randomDevices.Add(_handler.AllDevices["1BBECC"]);
                //if (_handler.AllDevices.ContainsKey("22A60C")) // front bedroom
                //    _randomDevices.Add(_handler.AllDevices["22A60C"]);
                //log.InfoFormat("Finished adding {0} random devices.", _randomDevices.Count);

                Thread serviceStartThread = new Thread(delegate()
                {
                    // ive confused myself a little while im running 2 versions of these threads
                    _wcfHostThread = new Thread(new ThreadStart(HostService));
                    _wcfHostThread.Start();

                    //_iisExpressThread = new Thread(new ThreadStart(RunIISExpress));
                    //_iisExpressThread.Start();

                    SetServiceState(State.SERVICE_RUNNING);

                });

                StartAirQualityMonitoring();

                serviceStartThread.Start();
                serviceStartThread.Join();
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while Starting Application.");
                log.Error(ex.Message);
                log.Error(ex.StackTrace);

            }
            log.Info("Finished Starting Insteon Service.");

        }

        void _handler_InsteonTrafficDetected(object sender, InsteonTrafficEventArgs e)
        {
            
        }

        void _handler_PartyDetected(object sender)
        {
            _insteonWebService.Party();
        }

        protected override void OnStop()
        {
            SetServiceState(State.SERVICE_STOP_PENDING);

            if (null != _iisProcess)
            {
                log.Info("Killing IIS process.");
                try
                {
                    _iisProcess.Kill();
                }
                catch (Exception) { }
            }

            _stop = true;

            if (null != _plm)
            {
                _plm.Close();
                _plm.Dispose();
            }

            if (null != _airQualityWorker)
            {
                _airQualityWorker.Stop();
            }
        }

        private void SetServiceState(State state)
        {
            IntPtr handle = this.ServiceHandle;
            _serviceStatus.currentState = (int)state;

            SetServiceStatus(handle, ref _serviceStatus);
        }

        private void RunIISExpress()
        {
            try
            {
                Thread iisHostThread = new Thread(delegate()
                {
                    log.Info("About to start IIS process");
                    StreamReader error = null;
                    StreamReader outputReader = null;
                    string errorOutput = "";
                    string output = "";
                    try
                    {
                        _iisProcess = new Process
                        {
                            EnableRaisingEvents = false,
                            StartInfo =
                            {
                                FileName = @"C:\Program Files\IIS Express\iisexpress.exe",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                Verb = "runas"
                            }
                        };

                        _iisProcess.Start();

                        log.InfoFormat("Started iis process {0}", _iisProcess.Id);

                        error = _iisProcess.StandardError;
                        outputReader = _iisProcess.StandardOutput;

                        output = outputReader.ReadToEnd();
                        errorOutput = error.ReadToEnd();

                        _iisProcess.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error occurred starting iis. Message: {0}, trace: {1}", ex.Message, ex.StackTrace);
                    }
                    finally
                    {
                        if (null != error)
                        {
                            error.Close();
                            error.Dispose();
                        }
                    }

                    if (_iisProcess.ExitCode == 0)
                    {
                        log.ErrorFormat("IIS Express process exited cleanly.");

                    }
                    else
                    {
                        log.ErrorFormat("IIS Express process exited with code {0}.  Output: {1}, Error: {2}", _iisProcess.ExitCode, output, errorOutput);

                    }
                });

                iisHostThread.Start();
                iisHostThread.Join();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error occurred in IIS host thread: {0}, {1}", ex.Message, ex.StackTrace);
            }
        }

        private void HostService()
        {
            Thread wcfHostThread = new Thread(delegate()
            {
                WebServiceHost insteonHost = null;

                try
                {
                    _insteonWebService = new InsteonWebService(_handler);
                    insteonHost = new WebServiceHost(_insteonWebService);
                    insteonHost.Open();
                    log.Info("Web Host Opened.");
                    while (!_stop)
                    {
                        Thread.Sleep(15000);
                        ProcessDeviceEvents();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    log.Error("Error occurred while WCF Host Application.");
                    log.Error(ex.Message);
                    log.Error(ex.StackTrace);

                }
            });
            wcfHostThread.Start();

            wcfHostThread.Join();

            SetServiceState(State.SERVICE_STOPPED);
        }

        private void ProcessDeviceEvents()
        {
            try
            {
                Device coachLights = _handler.AllDevices.Values.FirstOrDefault(d => d.Name == "coachLights");

                SolarTime solarTime = new SolarTime();
                TimeZoneInfo timezone = TimeZoneInfo.Local;
                double sunrise = solarTime.CalculateSunriseOrSunset(true, _latitude, _longitude, timezone.BaseUtcOffset.Hours, timezone.IsDaylightSavingTime(DateTime.Now));

                int sunriseHour = (int)Math.Floor(sunrise / 60);
                int sunriseMinute = (int)Math.Floor(sunrise - (60 * sunriseHour));
                int sunriseSeconds = (int)(60.0 * ((double)sunrise - Math.Floor(sunrise)));

                double sunset = solarTime.CalculateSunriseOrSunset(false, _latitude, _longitude, timezone.BaseUtcOffset.Hours, timezone.IsDaylightSavingTime(DateTime.Now));
                int sunsetHour = (int)Math.Floor(sunset / 60);
                int sunsetMinute = (int)Math.Floor(sunset - (60 * sunsetHour));
                int sunsetSecond = (int)(60.0 * ((double)sunset - Math.Floor(sunset)));

                TimeSpan sunriseTimeSpan = new TimeSpan(sunriseHour, sunriseMinute, sunriseSeconds);
                TimeSpan sunsetTimeSpan = new TimeSpan(sunsetHour, sunsetMinute, sunsetSecond);
                bool processSunset = !(_lastSunset.HasValue && _lastSunset.Value.AddHours(20) > DateTime.Now);
                bool processSunrise = !(_lastSunrise.HasValue && _lastSunrise.Value.AddHours(20) > DateTime.Now);

                if (processSunset && (DateTime.Now.TimeOfDay >= sunsetTimeSpan || DateTime.Now.TimeOfDay < sunriseTimeSpan))
                {
                    _lastSunset = DateTime.Now;

                    foreach (string key in _handler.AllDevices.Keys)
                    {
                        Device dev = _handler.AllDevices[key];

                        if (dev.IsOnAtSunset)
                        {
                            _handler.SendStandardCommand(dev.Address, Constants.STD_COMMAND_FAST_ON, 0x00, 0x0F);
                            _handler.ProcessSendingRelatedEvents(Constants.STD_COMMAND_FAST_ON, dev);
                            dev.Status = 1;
                            dev.LastOn = DateTime.Now;
                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                                string.Format("Turned on at sunset")));
                            Thread.Sleep(500);
                        }
                    }
                }

                if (processSunrise && (DateTime.Now.TimeOfDay >= sunriseTimeSpan && DateTime.Now.TimeOfDay < sunsetTimeSpan))
                {
                    _lastSunrise = DateTime.Now;
                    foreach (string key in _handler.AllDevices.Keys)
                    {
                        Device dev = _handler.AllDevices[key];

                        if (dev.IsOffAtSunrise)
                        {
                            _handler.SendStandardCommand(dev.Address, Constants.STD_COMMAND_FAST_OFF, 0x00, 0x0F);
                            _handler.ProcessSendingRelatedEvents(Constants.STD_COMMAND_FAST_OFF, dev);
                            dev.Status = 0;
                            dev.LastOff = DateTime.Now;
                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                                string.Format("Turned off at sunrise")));
                            Thread.Sleep(500);
                        }
                    }
                }

                // find all devices that are supporting random

                List<Device> randomDevices = _handler.AllDevices.Values.Where(d => d.IsRandomOn == true).ToList();

                foreach (Device randomDevice in randomDevices)
                {
                    // determine whether we are in the random window
                    if (!(DateTime.Now.Hour >= (randomDevice.RandomOnStart ?? 0) && (DateTime.Now.Hour < ((randomDevice.RandomOnStart ?? 0) + (randomDevice.RandomRunDuration ?? 0) % 24))))
                    {
                        // outside the window, just make sure we have ended the random by turning off
                        if (randomDevice.Status > 0 && randomDevice.LastOnWasRandom)
                        {
                            randomDevice.LastOnWasRandom = false;
                            randomDevice.LastRandomOnTime = null;
                            randomDevice.LastRandomOnLength = null;
                            randomDevice.LastOff = DateTime.Now;
                            randomDevice.Status = 0;
                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(randomDevice.Name, "Turning off after random schedule ended."));

                            _handler.SendStandardCommand(randomDevice.Address, Constants.STD_COMMAND_FAST_OFF, 0x00, 0x0F);
                        }

                        continue;
                    }

                    // below here we must be inside the random window for this device
                    if (randomDevice.Status > 0)
                    {
                        // did we turn it on?  if so we may need to turn it off now
                        if (randomDevice.LastOnWasRandom)
                        {
                            if (randomDevice.LastRandomOnTime.HasValue && randomDevice.LastRandomOnTime.Value.AddMinutes(randomDevice.LastRandomOnLength ?? 0) < DateTime.Now)
                            {
                                // turn off device, wipe out randomization settings
                                randomDevice.LastOnWasRandom = false;
                                randomDevice.LastRandomOnTime = null;
                                randomDevice.LastRandomOnLength = null;
                                randomDevice.LastOff = DateTime.Now;
                                randomDevice.Status = 0;
                                SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(randomDevice.Name, "Turning off from random schedule."));
                                _handler.SendStandardCommand(randomDevice.Address, Constants.STD_COMMAND_FAST_OFF, 0x00, 0x0F);
                            }
                        }
                        else // do nothing, we did not turn the device on
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // device is off, determine whether to turn it on
                        if (_random.Next(1, 100) <= (randomDevice.RandomOnChance ?? 0))
                        {
                            int randomOnDuration = _random.Next(randomDevice.RandomDurationMin ?? 5, randomDevice.RandomDurationMax ?? 15);
                            randomDevice.LastRandomOnLength = randomOnDuration;
                            randomDevice.LastRandomOnTime = DateTime.Now;
                            randomDevice.LastOnWasRandom = true;
                            randomDevice.LastOn = DateTime.Now;
                            randomDevice.Status = 100;

                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(randomDevice.Name, string.Format("Turned on random for {0} minutes.", randomOnDuration)));
                            _handler.SendStandardCommand(randomDevice.Address, Constants.STD_COMMAND_FAST_ON, 0xFF, 0x0F);
                        }
                    }
                }

                //Device frontDoorHigh = _handler.AllDevices.Values.FirstOrDefault(d => d.Name == "frontDoorHigh");
                //if (null != frontDoorHigh)
                //{
                //    if (frontDoorHigh.Status == 0)
                //    {
                //        if (DateTime.Now.Hour >= 21 || DateTime.Now.Hour < 1)
                //        {
                //            _insteonWebService.FastOn("frontdoorHigh");
                //            frontDoorHigh.Status = 1;
                //            frontDoorHigh.LastOn = DateTime.Now;
                //            log.Info(string.Format("Turned front door high light on at {0}", DateTime.Now));
                //            Thread.Sleep(500);
                //        }
                //    }
                //    else
                //    {
                //        if (DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 21)
                //        {
                //            _insteonWebService.Off("frontdoorHigh");
                //            frontDoorHigh.Status = 0;
                //            frontDoorHigh.LastOff = DateTime.Now;
                //            log.Info(string.Format("Turned front door high light off at {0}", DateTime.Now));
                //            Thread.Sleep(500);
                //        }
                //    }
                //}

                //Device backyard = _handler.AllDevices.Values.FirstOrDefault(d => d.Name == "backyard");
                //if (null != backyard)
                //{
                //    if (backyard.Status == 0)
                //    {
                //        if (DateTime.Now.Hour >= 22 || DateTime.Now.Hour < 6)
                //        {
                //            _insteonWebService.FastOn("backyard");
                //            backyard.Status = 1;
                //            backyard.LastOn = DateTime.Now;
                //            log.Info(string.Format("Turned backyard light on at {0}", DateTime.Now));
                //            Thread.Sleep(500);
                //        }
                //    }
                //    else
                //    {
                //        if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 22)
                //        {
                //            _insteonWebService.Off("backyard");
                //            backyard.Status = 0;
                //            backyard.LastOff = DateTime.Now;
                //            log.Info(string.Format("Turned backyard light off at {0}", DateTime.Now));
                //            Thread.Sleep(500);
                //        }
                //    }
                //}

                // handle random events
                //foreach (Device randomDevice in _randomDevices)
                //{
                //    if (DateTime.Now < new DateTime(2013, 8, 9, 7, 0, 0) || DateTime.Now > new DateTime(2013, 8, 22, 6, 0, 0))
                //        continue;

                //    if (randomDevice.Status == 1 && randomDevice.NextOff < DateTime.Now)
                //    {
                //        _insteonWebService.Off(randomDevice.Name);
                //        randomDevice.Status = 0;
                //        randomDevice.LastOff = DateTime.Now;
                //        log.Info(string.Format("Turned off random device {0} at {1}", randomDevice.Name, DateTime.Now));
                //        Thread.Sleep(500);

                //        // turn off partner switch
                //        if (randomDevice.Name == "kitchenMulti")
                //        {
                //            Device kitchenMultiSolo = _handler.AllDevices["192B89"]; // kitchenMultiSolo
                //            if (null != kitchenMultiSolo)
                //            {
                //                _insteonWebService.Off(kitchenMultiSolo.Name);
                //                kitchenMultiSolo.Status = 0;
                //                kitchenMultiSolo.LastOff = DateTime.Now;
                //                Thread.Sleep(500);
                //            }
                //        }
                //    }

                //    // only turn lights on at random during the night
                //    if (DateTime.Now.Hour >= 21 || DateTime.Now.Hour < 5)
                //    {
                //        if (randomDevice.Status != 1)
                //        {
                //            // 2% chance to turn something on
                //            if (_random.Next(1, 100) > 98)
                //            {
                //                int randomDuration = _random.Next(7, 36);
                //                log.Info(string.Format("Decided to turn on device {0} for {1} minutes.", randomDevice.Name, randomDuration));

                //                randomDevice.NextOff = DateTime.Now.AddMinutes(randomDuration);

                //                _insteonWebService.FastOn(randomDevice.Name);
                //                randomDevice.Status = 1;
                //                randomDevice.LastOn = DateTime.Now;
                //                Thread.Sleep(500);

                //                if ("kitchenMulti" == randomDevice.Name)
                //                {
                //                    Device kitchenMultiSolo = _handler.AllDevices["192B89"]; // kitchen multi solo
                //                    if (null != kitchenMultiSolo)
                //                    {
                //                        _insteonWebService.FastOn(kitchenMultiSolo.Name);
                //                        kitchenMultiSolo.Status = 1;
                //                        kitchenMultiSolo.LastOn = DateTime.Now;
                //                        Thread.Sleep(500);
                //                    }
                //                }
                //            }
                //        }   
                //    }
                //} 
            }
            catch (Exception ex)
            {
                log.Error("Error during device event loop");
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
            }
        }

        private void StartAirQualityMonitoring()
        {
            try
            {
                _airQualityWorker = new MainWorker();
                Dictionary<string, string> sensorDictionary = new Dictionary<string, string>();
                sensorDictionary.Add("mbr", "http://192.168.222.166:8085/stats");
                sensorDictionary.Add("family", "http://192.168.222.167:8085/stats");
                sensorDictionary.Add("mobile", "http://192.168.222.225:8085/stats");

                _airQualityWorker.Initialize(sensorDictionary);
                _airQualityWorker.Start();
            }
            catch (Exception ex)
            {
                log.Error("Error while starting air quality monitors");
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_STATUS
    {
        public int serviceType;
        public int currentState;
        public int controlsAccepted;
        public int win32ExitCode;
        public int serviceSpecificExitCode;
        public int checkPoint;
        public int waitHint;
    }

    public enum State
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007
    }
}
