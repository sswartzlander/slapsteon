using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.ServiceModel.Web;
using Insteon.Library;
using System.Configuration;
using log4net;

namespace Insteon
{
    class Program
    {

        private static string _serialPort = "COM4";
        private static SerialPort _plm;
        private static object _serialReadLock = new object();
        private static List<Device> _allDevices = new List<Device>();
        private static InsteonHandler _handler;


        public static SerialPort PLM { get { return _plm; } }
 //       public static object SerialLock { get { return _serialReadLock; } }
        private static readonly ILog log = LogManager.GetLogger("Insteon");

        static void Main(string[] args)
        {

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

            try
            {
                log4net.Config.XmlConfigurator.Configure();

                string serialPort = ConfigurationManager.AppSettings["SerialPort"];
                if (!string.IsNullOrEmpty(serialPort))
                    _serialPort = serialPort;

                _handler = new InsteonHandler(_serialPort, _allDevices);
                _handler.EnableMonitorMode();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception occurred in main program.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

            }
            finally
            {
                if (null != _plm)
                {
                    _plm.Close();
                    _plm.Dispose();
                }
            }

        }

        
    }
}
