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
//        private static object _serialReadLock = new object();

        public static SerialPort PLM { get { return _plm; } }
 //       public static object SerialLock { get { return _serialReadLock; } }
        private static readonly ILog log = LogManager.GetLogger("Insteon");

        static void Main(string[] args)
        {
            
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                log.Info("Test logging.");
                log.Debug("Test logging.");

                string serialPort = ConfigurationManager.AppSettings["SerialPort"];
                if (!string.IsNullOrEmpty(serialPort))
                    _serialPort = serialPort;

                //bool enableMonitorMode = false;
                //string monitorModeSetting = ConfigurationManager.AppSettings["EnableMonitorMode"];
                //if (!string.IsNullOrEmpty(monitorModeSetting))
                //    if (!bool.TryParse(monitorModeSetting, out enableMonitorMode))
                //        Console.WriteLine("Could not load monitor mode setting.");

                _plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                Console.WriteLine("Opening serial port " + _serialPort);
                _plm.Open();
                Console.WriteLine("Successfully connected to PLM.");

                Thread wcfHostThread = new Thread(delegate()
                {
                    WebServiceHost insteonHost = null;

                    try
                    {
                        InsteonWebService insteonWebService = new InsteonWebService(_plm);
                        insteonHost = new WebServiceHost(insteonWebService);
                        insteonHost.Open();
                        while (true)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception ex)
                    {

                    }
                });
                wcfHostThread.Start();

                //if (enableMonitorMode)
                //{
                //    Thread monitorModeThread = new Thread(delegate()
                //    {
                //        TryMonitorMode();
                //    });

                //    monitorModeThread.Start();
                //    monitorModeThread.Join();
                //}

                wcfHostThread.Join();
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
