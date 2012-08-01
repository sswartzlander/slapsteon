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
        private Thread _wcfHostThread;
        private SERVICE_STATUS _serviceStatus;
        private static readonly ILog log = LogManager.GetLogger("Insteon");
        private List<Device> _allDevices = new List<Device>();
        private List<Device> _randomDevices = new List<Device>();

        private InsteonWebService _insteonWebService;
        private Random _random;
        public InsteonWindowsService()
        {
            log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();

            // set up devices
            _allDevices.Add(new Device("coachLights", new DeviceAddress(0x17, 0xF3, 0x23)));
            _allDevices.Add(new Device("gameroomDimmer", new DeviceAddress(0x1B, 0xBC, 0xC0)));
            _allDevices.Add(new Device("livingroomDimmer", new DeviceAddress(0x1B, 0xBE, 0xCC)));
            _allDevices.Add(new Device("mbrDimmer", new DeviceAddress(0x1B, 0xB0, 0xB9)));
            _allDevices.Add(new Device("mbrMulti", new DeviceAddress(0x19, 0x2B, 0xD4)));
            _allDevices.Add(new Device("kitchenMultiSolo", new DeviceAddress(0x19, 0x2B, 0x89)));
            _allDevices.Add(new Device("kitchenMulti", new DeviceAddress(0x19, 0x2A, 0x4D)));
            _allDevices.Add(new Device("breakfastDimmer", new DeviceAddress(0x1B, 0xBF, 0x6E)));
            _allDevices.Add(new Device("frontDoorHigh", new DeviceAddress(0x19, 0x2B, 0x83)));
            _allDevices.Add(new Device("plm", new DeviceAddress(0x19, 0x77, 0x51)));

            _randomDevices.Add(new Device("livingroomDimmer", new DeviceAddress(0x1B, 0xBE, 0xCC)));            
            _randomDevices.Add(new Device("kitchenMulti", new DeviceAddress(0x19, 0x2A, 0x4D)));
            _randomDevices.Add(new Device("breakfastDimmer", new DeviceAddress(0x1B, 0xBF, 0x6E)));

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

                _plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                _plm.Open();

                log.Info(string.Format("Opened {0} successfully.", _serialPort));

                SetServiceState(State.SERVICE_START_PENDING);

                Thread serviceStartThread = new Thread(delegate()
                {
                    _wcfHostThread = new Thread(new ThreadStart(HostService));
                    _wcfHostThread.Start();

                    SetServiceState(State.SERVICE_RUNNING);

                });

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

        protected override void OnStop()
        {
            SetServiceState(State.SERVICE_STOP_PENDING);

            _stop = true;

            if (null != _plm)
            {
                _plm.Close();
                _plm.Dispose();
            }
        }

        private void SetServiceState(State state)
        {
            IntPtr handle = this.ServiceHandle;
            _serviceStatus.currentState = (int)state;

            SetServiceStatus(handle, ref _serviceStatus);
        }

        private void HostService()
        {
            Thread wcfHostThread = new Thread(delegate()
            {
                WebServiceHost insteonHost = null;

                try
                {
                    _insteonWebService = new InsteonWebService(_plm);
                    insteonHost = new WebServiceHost(_insteonWebService);
                    insteonHost.Open();
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
                Device coachLights = _allDevices.FirstOrDefault(d => d.Name == "coachLights");

                if (null != coachLights)
                {
                    if (coachLights.Status != 1)
                    {
                        if (DateTime.Now.Hour >= 20 || DateTime.Now.Hour < 7)
                        {
                            _insteonWebService.FastOn(_plm, coachLights.Address);
                            coachLights.Status = 1;
                            coachLights.LastOn = DateTime.Now;
                            log.Info(string.Format("Turned coach lights on at {0}", DateTime.Now));
                            Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 20)
                        {
                            _insteonWebService.FastOff(_plm, coachLights.Address);
                            coachLights.Status = 0;
                            coachLights.LastOff = DateTime.Now;
                            log.Info(string.Format("Turned coach lights off at {0}", DateTime.Now));
                            Thread.Sleep(500);
                        }
                    }
                }

                Device frontDoorHigh = _allDevices.FirstOrDefault(d => d.Name == "frontDoorHigh");
                if (null != frontDoorHigh)
                {
                    if (frontDoorHigh.Status != 1)
                    {
                        if (DateTime.Now.Hour >= 21 || DateTime.Now.Hour < 1)
                        {
                            _insteonWebService.FastOn(_plm, frontDoorHigh.Address);
                            frontDoorHigh.Status = 1;
                            frontDoorHigh.LastOn = DateTime.Now;
                            log.Info(string.Format("Turned front door high light on at {0}", DateTime.Now));
                            Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        if (DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 21)
                        {
                            _insteonWebService.FastOff(_plm, frontDoorHigh.Address);
                            frontDoorHigh.Status = 0;
                            frontDoorHigh.LastOff = DateTime.Now;
                            log.Info(string.Format("Turned front door high light off at {0}", DateTime.Now));
                            Thread.Sleep(500);
                        }
                    }
                }
                /*
                // handle random events
                foreach (Device randomDevice in _randomDevices)
                {
                    if (randomDevice.Status == 1 && randomDevice.NextOff < DateTime.Now)
                    {
                        _insteonWebService.FastOff(_plm, randomDevice.Address);
                        randomDevice.Status = 0;
                        randomDevice.LastOff = DateTime.Now;
                        log.Info(string.Format("Turned off random device {0} at {1}", randomDevice.Name, DateTime.Now));
                        Thread.Sleep(500);

                        // turn off partner switch
                        if (randomDevice.Name == "kitchenMulti")
                        {
                            Device kitchenMultiSolo = _allDevices.FirstOrDefault(d => d.Name == "kitchenMultiSolo");
                            if (null != kitchenMultiSolo)
                            {
                                _insteonWebService.FastOff(_plm, kitchenMultiSolo.Address);
                                kitchenMultiSolo.Status = 0;
                                kitchenMultiSolo.LastOff = DateTime.Now;
                                Thread.Sleep(500);
                            }
                        }
                    }

                    // only turn lights on at random during the night
                    if (DateTime.Now.Hour >= 21 || DateTime.Now.Hour < 5)
                    {
                        if (randomDevice.Status != 1)
                        {
                            // 2% chance to turn something on
                            if (_random.Next(1, 100) > 98)
                            {
                                int randomDuration = _random.Next(7, 36);
                                log.Info(string.Format("Decided to turn on device {0} for {1} minutes.", randomDevice.Name, randomDuration));

                                randomDevice.NextOff = DateTime.Now.AddMinutes(randomDuration);

                                _insteonWebService.FastOn(_plm, randomDevice.Address);
                                randomDevice.Status = 1;
                                randomDevice.LastOn = DateTime.Now;
                                Thread.Sleep(500);

                                if ("kitchenMulti" == randomDevice.Name)
                                {
                                    Device kitchenMultiSolo = _allDevices.FirstOrDefault(d => d.Name == "kitchenMultiSolo");
                                    if (null != kitchenMultiSolo)
                                    {
                                        _insteonWebService.FastOn(_plm, kitchenMultiSolo.Address);
                                        kitchenMultiSolo.Status = 1;
                                        kitchenMultiSolo.LastOn = DateTime.Now;
                                        Thread.Sleep(500);
                                    }
                                }
                            }
                        }
                    }
                } */
            }
            catch (Exception ex)
            {
                log.Error("Error during device event loop");
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
