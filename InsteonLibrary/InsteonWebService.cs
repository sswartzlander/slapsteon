using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Configuration;
using System.ServiceModel;
using log4net;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Insteon.Devices;
using Newtonsoft.Json;
using System.ServiceModel.Channels;

namespace Insteon.Library
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class InsteonWebService : IInsteonWebService
    {
        private static readonly ILog log = LogManager.GetLogger("Insteon");
        private readonly string _serialPort = "COM4";

        private static readonly Guid _x = new Guid("6923dddf-77f6-4605-8e77-246187c49646");
        private static readonly Guid _y = new Guid("78929c13-d859-4b85-8b4d-10032084e4f2");
        private static readonly Guid _z = new Guid("5e2e1f42-c899-4b8a-83c1-a385037f906c");
        private static InsteonHandler _handler;

        private static object _serialLock = new object();

        public InsteonWebService(InsteonHandler handler)
        {
            _handler = handler;
        }

        public void Alarm(string x, string y)
        {
            // x = 6923dddf-77f6-4605-8e77-246187c49646
            // y = 78929c13-d859-4b85-8b4d-10032084e4f2
            try
            {
                Guid xGuid = new Guid(x);
                Guid yGuid = new Guid(y);

                if (xGuid != _x && yGuid != _y)
                    return;
                FastOn("livingroomDimmer");

                Thread.Sleep(500);

                FastOn("kitchenMulti");

                Thread.Sleep(500);

                FastOn("breakfastDimmer");

                StreamReader outputReader;
                log.Info("Playing Alarm sound now.");

                ProcessStartInfo plinkStartInfo = new ProcessStartInfo("C:\\plink.exe");
                plinkStartInfo.Arguments = "steve@192.168.222.223 -P 59727 -i C:\\putty_privkey.ppk Alarm";
                plinkStartInfo.RedirectStandardOutput = true;
                plinkStartInfo.RedirectStandardInput = true;
                //plinkStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                plinkStartInfo.UseShellExecute = false;
                plinkStartInfo.CreateNoWindow = true;

                Process proc = Process.Start(plinkStartInfo);

               // outputReader = proc.StandardOutput;

                proc.WaitForExit(5000);

                //string s = outputReader.ReadToEnd();
                log.Info("Completed playing alarm sound.");
                //log.Info(s);

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error occurred playing Alarm: {0}", ex.Message));
                log.Error(ex.StackTrace);
            }
        }

        public void Alarm2(string z)
        {
            // z = 5e2e1f42-c899-4b8a-83c1-a385037f906c
            try
            {
                Guid zGuid = new Guid(z);

                if (zGuid != _z)
                    return;

                StreamReader outputReader;
                log.Info("Playing Alarm sound now.");

                ProcessStartInfo plinkStartInfo = new ProcessStartInfo("C:\\plink.exe");
                plinkStartInfo.Arguments = "steve@192.168.222.113 -i C:\\putty_privkey.ppk Alarm";
                plinkStartInfo.RedirectStandardOutput = true;
                plinkStartInfo.RedirectStandardInput = true;
                //plinkStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                plinkStartInfo.UseShellExecute = false;
                plinkStartInfo.CreateNoWindow = true;

                Process proc = Process.Start(plinkStartInfo);

                // outputReader = proc.StandardOutput;

                proc.WaitForExit(5000);

                //string s = outputReader.ReadToEnd();
                log.Info("Completed playing alarm sound.");
                //log.Info(s);

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error occurred playing Alarm: {0}", ex.Message));
                log.Error(ex.StackTrace);
            }
        }

        public void Level1Alert()
        {
            try
            {
                FastOn("officeOutlet");

                Thread.Sleep(2000);
                
                RampOff("officeFan", "64");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed while playing Level1Alert.  Message: {0}\nStacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        public Device GetDevice(string name)
        {
            Device device = null;

            foreach (string key in _handler.AllDevices.Keys)
            {
                if (_handler.AllDevices[key].Name.ToUpper() == name.ToUpper())
                {
                    device = _handler.AllDevices[key];
                    break;
                }
            }

            return device;
        }

        public SlapsteonDevice[] GetDevices()
        {
            List<SlapsteonDevice> slapsteonDevices = new List<SlapsteonDevice>();
            try
            {
                List<Device> devices = _handler.AllDevices.Values.ToList();



                foreach (Device device in devices)
                {
                    SlapsteonDevice slapsteonDevice = new SlapsteonDevice()
                    {
                        Address = device.AddressString,
                        LastOff = device.LastOff,
                        LastOn = device.LastOn,
                        Name = device.Name,
                        Status = device.Status.ToString(),
                        IsPLM = device is PLMDevice,
                        IsDimmable = device is DimmerDevice,
                        IsFan = device is FanDevice
                    };

                    slapsteonDevices.Add(slapsteonDevice);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error serializing devices: {0}\n{1}", ex.Message, ex.StackTrace);
            }

            return slapsteonDevices.ToArray();
        }

        public Device[] GetDevices2()
        {
            try
            {
                //return JsonConvert.SerializeObject(_handler.AllDevices.Values.ToList(), Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                return _handler.AllDevices.Values.Where(d => !d.IsSlaveDevice).ToArray();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error serializing devices: {0}\n{1}", ex.Message, ex.StackTrace);
            }

            return null;
        }

        public string Devices3()
        {
            return JsonConvert.SerializeObject(_handler.AllDevices.Values.Where(d=>!d.IsSlaveDevice).ToList(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All});

        }

        public void FastOn(string device)
        {
            FastOnWeb(device, GetClientIPAddress());
        }

        public void FastOnWeb(string device, string ip)
        {
            Device dev = GetDevice(device);

            if (null == dev)
                return;

            dev.Status = 100;
            dev.LastOn = DateTime.Now;

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Turned on API {0}", ip)));

            _handler.SendStandardCommand(dev.Address, Constants.STD_COMMAND_FAST_ON, 0x00, 0x07);
        }

        public void On(string device, string level)
        {
            OnWeb(device, level, GetClientIPAddress());
        }

        public void OnWeb(string device, string level, string ip)
        {
            int levelValue;
            if (!int.TryParse(level, out levelValue))
            {
                log.Warn("Invalid level value: " + level);
                return;
            }

            Device dev = GetDevice(device);

            if (null == dev)
                return;

            byte byteLevel = (byte)(levelValue * 255 / 100);

            dev.Status = levelValue;
            dev.LastOn = DateTime.Now;

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Turned on to {0}% by API {1}", level, ip)));

            _handler.SendStandardCommand(dev.Address, Constants.STD_COMMAND_ON, byteLevel, 0x07);
            _handler.ProcessSendingRelatedEvents(Constants.STD_COMMAND_ON, dev);
        }

        public void RampOn(string device, string level, string rate)
        {
            RampOnWeb(device, level, rate, GetClientIPAddress());
        }

        public void RampOnWeb(string device, string level, string rate, string ip)
        {
            int levelValue;
            if (!int.TryParse(level, out levelValue))
            {
                log.Warn("Invalid level value: " + level);
                return;
            }

            int rateValue;
            if (!int.TryParse(rate, out rateValue))
            {
                log.Warn("Invalid rate value: " + rate);
                return;
            }


            Device dev = GetDevice(device);

            if (null == dev)
                return;

            dev.Status = levelValue;
            dev.LastOn = DateTime.Now;

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Ramped on to {0}% at rate {1} by API {2}", level, rate, GetClientIPAddress())));


            // command2 is brightness & ramp rate... all in 1 byte... 
            // so there are only 16 possible increments of each

            byte brighthessByte = (byte)((byte)(levelValue * 100 / 625) << 4);
            byte rampRateByte = (byte)(rateValue  * 100 / 625);

            log.Debug(string.Format("Calling Ramp On, Level: {0} (0x{1}), Rate: {2}(0x{3})", levelValue, brighthessByte.ToString("X"), rateValue, rampRateByte.ToString("X").PadRight(2,'0')));
            byte command2 = (byte)(brighthessByte | rampRateByte);

            _handler.SendStandardCommand(dev.Address, Constants.STD_COMMAND_LIGHT_RAMP_ON, command2, 0x07);
            _handler.ProcessSendingRelatedEvents(Constants.STD_COMMAND_ON, dev);
        }

        public void Off(string device)
        {
            OffWeb(device, GetClientIPAddress());
        }

        public void OffWeb(string device, string ip)
        {
            Device dev = GetDevice(device);

            if (null == dev)
                return;

            dev.Status = 0;
            dev.LastOff = DateTime.Now;

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Turned off by API {0}", ip)));
            
            _handler.SendStandardCommand(dev.Address, Constants.STD_COMMAND_FAST_OFF, 0x00, 0x07);
            _handler.ProcessSendingRelatedEvents(Constants.STD_COMMAND_OFF, dev);

        }

        public void RampOff(string device, string rate)
        {
            RampOffWeb(device, rate, GetClientIPAddress());
        }

        public void RampOffWeb(string device, string rate, string ip)
        {
            int rateValue;
            if (!int.TryParse(rate, out rateValue))
            {
                log.Warn("Invalid rate value: " + rate);
                return;
            }

            Device dev = GetDevice(device);

            if (null == dev)
                return;

            // command2 is brightness & ramp rate... all in 1 byte... 
            // so there are only 16 possible increments of each

            dev.Status = 0;
            dev.LastOff = DateTime.Now;

            byte rampRateByte = (byte)(rateValue * 100 / 625);

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Ramped off at rate {0} by API {1}", rate, ip)));



            _handler.SendStandardCommand(dev.Address, Constants.STD_COMMAND_LIGHT_RAMP_OFF, rampRateByte, 0x07);
            _handler.ProcessSendingRelatedEvents(Constants.STD_COMMAND_OFF, dev);
        }

        public void On2(string device, string level)
        {
            On2Web(device, level, GetClientIPAddress());
        }

        public void On2Web(string device, string level, string ip)
        {
            int levelValue;
            if (!int.TryParse(level, out levelValue))
            {
                log.Warn("Invalid level value: " + level);
                return;
            }

            Device dev = GetDevice(device);

            if (null == dev || !(dev is FanDevice))
                return;

            // not sure why the easier computation converts 100 to 0xFE
            byte byteLevel = (byte)(levelValue * 255 / 100);

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Turned Fan to {0}% by API {1}", level, ip)));



            _handler.SendExtendedCommand(dev.Address, Constants.STD_COMMAND_ON, byteLevel, 0x1F, false, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);

        }

        public void SetCoolMode(string device, string ip)
        {
            ThermostatDevice dev = GetDevice(device) as ThermostatDevice;
            if (null == dev)
                return;
            dev.CurrentMode = ThermostatDevice.Mode.Cooling;

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Thermostat changed to Cooling by API {0}", ip)));


            _handler.SendExtendedCommand(dev.Address, Constants.STD_COMMAND_THERMOSTAT_CONTROL, Constants.THERMOSTAT_CONTROL_SET_COOL_MODE, 0x1F, true);
        }


        public void SetHeatMode(string device, string ip)
        {
            ThermostatDevice dev = GetDevice(device) as ThermostatDevice;
            if (null == dev)
                return;
            dev.CurrentMode = ThermostatDevice.Mode.Heating;


            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Thermostat changed to Heating by API {0}", ip)));


            _handler.SendExtendedCommand(dev.Address, Constants.STD_COMMAND_THERMOSTAT_CONTROL, Constants.THERMOSTAT_CONTROL_SET_HEAT_MODE, 0x1F, true);

        }

        public void SetPointUp(string device, string ip)
        {
            ThermostatDevice dev = GetDevice(device) as ThermostatDevice;
            if (null == dev)
                return;
           
            int newSetPoint = 0;

            if (dev.CurrentMode == ThermostatDevice.Mode.Cooling)
            {
                newSetPoint = ++dev.CoolSetPoint;
            }
            else if (dev.CurrentMode == ThermostatDevice.Mode.Heating)
            {
                newSetPoint = ++dev.HeatSetPoint;
            }

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Thermostat set point increased to {0} by API {1}", newSetPoint, ip)));


            _handler.SendExtendedCommand(dev.Address, Constants.STD_COMMAND_THERMOSTAT_TEMP_UP, 0x02, 0x1F, true);

        }

        public void SetPointDown(string device, string ip)
        {
            ThermostatDevice dev = GetDevice(device) as ThermostatDevice;
            if (null == dev)
                return;

            int newSetPoint = 0;

            if (dev.CurrentMode == ThermostatDevice.Mode.Cooling)
            {
                newSetPoint = --dev.CoolSetPoint;
            }
            else if (dev.CurrentMode == ThermostatDevice.Mode.Heating)
            {
                newSetPoint = --dev.HeatSetPoint;
            }

            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(dev.Name,
                string.Format("Thermostat set point decreased to {0} by API {1}", newSetPoint, ip)));


            _handler.SendExtendedCommand(dev.Address, Constants.STD_COMMAND_THERMOSTAT_TEMP_DOWN, 0x02, 0x1F, true);

        }

        public SlapsteonEventLogEntry[] Log()
        {
            return SlapsteonEventLog.ToArray();
        }

        public void Party()
        {
            try
            {
                ProcessStartInfo plinkStartInfo = new ProcessStartInfo("C:\\plink.exe");
                plinkStartInfo.Arguments = "steve@192.168.222.223 -P 59727 -i C:\\putty_privkey.ppk PartyRock";
                plinkStartInfo.RedirectStandardOutput = true;
                plinkStartInfo.RedirectStandardInput = true;
                //plinkStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                plinkStartInfo.UseShellExecute = false;
                plinkStartInfo.CreateNoWindow = true;

                Process proc = Process.Start(plinkStartInfo);

                // outputReader = proc.StandardOutput;

                proc.WaitForExit(5000);

                //string s = outputReader.ReadToEnd();
                log.Info("Completed playing alarm sound.");
                //log.Info(s);

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error occurred playing Alarm: {0}", ex.Message));
                log.Error(ex.StackTrace);
            }
        }

        public void EnterLinkingMode()
        {
            _handler.SendStartAllLink();
        }

        public void CancelLinkingMode()
        {
            _handler.SendCancelAllLink();
        }

        public void Unlink()
        {

            _handler.SendDeleteAllLink();
        }


        public void SendStandardCommand(string command)
        {
            byte[] commandBytes = ConvertCommandStringToBytes(command);

            _handler.SendStandardCommand(new DeviceAddress(commandBytes[2],commandBytes[3],commandBytes[4]), commandBytes[6], commandBytes[7], commandBytes[5]);
        }

        public void GetIMFirst()
        {
            _handler.GetFirstIMLink();
        }

        public void GetIMNext()
        {
            _handler.GetNextIMLink();
        }

        public void SendExtendedCommand(string command)
        {
            byte[] commandBytes = ConvertCommandStringToBytes(command);

            _handler.SendExtendedCommand(new DeviceAddress(commandBytes[2], commandBytes[3], commandBytes[4]), commandBytes[6], commandBytes[7], commandBytes[5], false,
                commandBytes[8], commandBytes[9], commandBytes[10], commandBytes[11], commandBytes[12], commandBytes[13], commandBytes[14],
                commandBytes[15], commandBytes[16], commandBytes[17], commandBytes[18], commandBytes[19], commandBytes[20], commandBytes[21]);
        }

        public void SendExtendedCommandChecksum(string command, string checksum)
        {
            byte[] commandBytes = ConvertCommandStringToBytes(command);

            bool check = false;
            if (!bool.TryParse(checksum, out check))
                check = false;
            _handler.SendExtendedCommand(new DeviceAddress(commandBytes[2], commandBytes[3], commandBytes[4]), commandBytes[6], commandBytes[7], commandBytes[5], check,
                commandBytes[8], commandBytes[9], commandBytes[10], commandBytes[11], commandBytes[12], commandBytes[13], commandBytes[14],
                commandBytes[15], commandBytes[16], commandBytes[17], commandBytes[18], commandBytes[19], commandBytes[20], commandBytes[21]);

        }

        public void SendIMManageLinkCommand(string command)
        {
            byte[] commandBytes = ConvertCommandStringToBytes(command);

            _handler.ManageIMAllLinkRecord(commandBytes);
        }

        public LinkRecord[] GetIMLinks()
        {
            return _handler.IMLinks.ToArray();
        }

        private string GetClientIPAddress()
        {
            string clientIP = "0.0.0.0";

            try
            {
                OperationContext context = OperationContext.Current;
                MessageProperties prop = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                clientIP = endpoint.Address;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error trying to get client ip address.  Message: {0}\n{1}",
                    ex.Message, ex.StackTrace);
            }

            return clientIP;
        }

        private byte[] ConvertCommandStringToBytes(string command)
        {
            if (command.Length % 2 != 0)
                return null;

            int commandLength = command.Length / 2;
            byte[] commandBytes = new byte[commandLength];

            for (int i = 0; i < commandLength; i++)
            {
                byte b = StringToByte(command.Substring(i * 2, 2));
                commandBytes[i] = b;
            }

            return commandBytes;
        }

        private byte StringToByte(string input)
        {
            byte retVal = 0x00;

            if (string.IsNullOrEmpty(input))
                return 0x00;

            string trimmed = null;
            if (input.StartsWith("0x"))
                trimmed = input.Substring(2);
            else if (input.Contains('x'))
                trimmed = input.Substring(input.IndexOf('x') + 1);
            else trimmed = input;

            if (trimmed.Length > 2)
                trimmed = trimmed.Substring(0, 2);

            retVal = (byte)Int32.Parse(trimmed, System.Globalization.NumberStyles.HexNumber);

            return retVal;
        }
    }
}
