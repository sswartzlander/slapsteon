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

        public static SerialPort PLM { get { return _plm; } }
 //       public static object SerialLock { get { return _serialReadLock; } }
        private static readonly ILog log = LogManager.GetLogger("Insteon");

        static void Main(string[] args)
        {
            
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                string serialPort = ConfigurationManager.AppSettings["SerialPort"];
                if (!string.IsNullOrEmpty(serialPort))
                    _serialPort = serialPort;

                bool enableMonitorMode = true;
                //string monitorModeSetting = ConfigurationManager.AppSettings["EnableMonitorMode"];
                //if (!string.IsNullOrEmpty(monitorModeSetting))
                //    if (!bool.TryParse(monitorModeSetting, out enableMonitorMode))
                //        Console.WriteLine("Could not load monitor mode setting.");

                _plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                Console.WriteLine("Opening serial port " + _serialPort);
                _plm.Open();
                Console.WriteLine("Successfully connected to PLM.");



                if (enableMonitorMode)
                {
                    Thread monitorModeThread = new Thread(delegate()
                    {
                        TryMonitorMode();
                    });

                    monitorModeThread.Start();
                    monitorModeThread.Join();
                }

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

        private static void TryMonitorMode()
        {

            byte[] cmdBytes = new byte[3];
            cmdBytes[0] = 0x02;
            cmdBytes[1] = 0x6B;
            cmdBytes[2] = 0x40; // 1000


            _plm.Write(cmdBytes, 0, 3);

            int numberOfBytesToRead = _plm.BytesToRead;

            byte[] bytesRead = new byte[numberOfBytesToRead];

            _plm.Read(bytesRead, 0, numberOfBytesToRead);

            string byteString = BitConverter.ToString(bytesRead);

            while (true)
            {
                // lock here so we can let specific operations prevent this from stealing output
                lock (_serialReadLock)
                {
                    numberOfBytesToRead = _plm.BytesToRead;

                    if (numberOfBytesToRead > 0)
                    {
                        bytesRead = new byte[numberOfBytesToRead];
                        _plm.Read(bytesRead, 0, numberOfBytesToRead);
                        byteString = BitConverter.ToString(bytesRead);
                        Console.WriteLine(DateTime.Now.ToString() + ": " + byteString);
                        log.Info(byteString);
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
