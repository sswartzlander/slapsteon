using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.IO.Ports;
using System.Threading;
using Insteon.Library.Configuration;
using System.Configuration;
using System.Xml.Serialization;
using System.IO;
using Insteon.Devices;

namespace Insteon.Library
{
    public class InsteonHandler
    {

        public delegate void InsteonTrafficHandler(object sender, InsteonTrafficEventArgs e);
        public delegate void PartyHandler(object sender);
        public event InsteonTrafficHandler InsteonTrafficDetected;
        public event PartyHandler PartyDetected;

        private SerialPort _plm;
        private static readonly ILog log = LogManager.GetLogger("Insteon");
        private Thread _monitorModeThread;

        private bool _gettingExtendedStatus = false;
        private bool _gettingStatus = false;
        private bool _gettingOpFlags = false;
        private object _statusSyncObject = new object();

        private bool _gettingThermostat = false;
        private bool _gettingTemp = false;
        private bool _gettingSetPoint = false;
        private bool _gettingHumidity = false;
        private bool _gettingThermostatExtended = false;

        private string _deviceALDBPath = @"C:\Insteon\DeviceALDB.xml";
        private DateTime _lastALDBRecordTime;
        private bool _aldbFinishedForDevice = false;

        private EventWaitHandle _aldbEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private EventWaitHandle _statusEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private EventWaitHandle _extendedGetWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Dictionary<string,Device> _allDevices = new Dictionary<string,Device>();
        private ALDBLibrary _aldbLibrary = new ALDBLibrary();

        private List<LinkRecord> _imLinks = new List<LinkRecord>();
        public Dictionary<string, Device> AllDevices
        {
            get
            {
                return _allDevices;
            }
        }

        public ALDBLibrary AllDeviceALDB
        {
            get
            {
                return _aldbLibrary;
            }
        }

        public List<LinkRecord> IMLinks { get { return _imLinks; } }

        public InsteonHandler(string comPort, bool saveALDB)
        {
            // read the configuration file
            SlapsteonConfigurationSection slapsteonConfiguration = 
                ConfigurationManager.GetSection("slapsteon") as SlapsteonConfigurationSection;

            try
            {
                _plm = new SerialPort(comPort, 19200, Parity.None, 8, StopBits.One);
                log.Info("Opening serial port " + comPort);
                _plm.Open();
                log.Info("Successfully connected to PLM.");

            }
            catch (Exception ex)
            {
                log.Error("Error opening serial port", ex);
                throw ex;
            }
            _plm.DataReceived += new SerialDataReceivedEventHandler(_plm_DataReceived);
            Thread.Sleep(500);
            this.SetMonitorMode();

            string deviceALDBPathSetting = ConfigurationManager.AppSettings["deviceXMLPath"];
            if (!string.IsNullOrEmpty(deviceALDBPathSetting))
                _deviceALDBPath=deviceALDBPathSetting;

            if (null == slapsteonConfiguration)
                throw new ConfigurationErrorsException("The configuration needs a <slapsteon> section.");

            foreach (SlapsteonDeviceConfigurationElement element in slapsteonConfiguration.SlapsteonDevices)
            {
                string deviceName = element.Name;

                byte address1 = StringToByte(element.Address.Substring(0, 2));
                byte address2 = StringToByte(element.Address.Substring(2, 2));
                byte address3 = StringToByte(element.Address.Substring(4, 2));
                
                DeviceAddress deviceAddress= new DeviceAddress(address1, address2, address3);
                Device dev = null;
                if ((element.IsKPL.HasValue && element.IsKPL.Value) && (!element.IsDimmable.HasValue || !element.IsDimmable.Value))
                    dev = new MultiButtonRelayDevice(deviceName, deviceAddress);
                else if ((element.IsKPL.HasValue && element.IsKPL.Value) && (element.IsDimmable.HasValue && element.IsDimmable.Value))
                    dev = new MultiButtonDimmerDevice(deviceName, deviceAddress);
                else if (element.IsBatteryDevice.HasValue && element.IsBatteryDevice.Value)
                    dev = new SensorDevice(deviceName, deviceAddress);
                else if (element.IsFan.HasValue && element.IsFan.Value)
                    dev = new FanDevice(deviceName, deviceAddress);
                else if (element.IsDimmable.HasValue && element.IsDimmable.Value)
                    dev = new DimmerDevice(deviceName, deviceAddress);
                else if (element.IsPLM.HasValue && element.IsPLM.Value)
                    dev = new PLMDevice(deviceName, deviceAddress);
                else if (element.IsIODevice.HasValue && element.IsIODevice.Value)
                    dev = new IODevice(deviceName, deviceAddress);
                else if (element.IsThermostat.HasValue && element.IsThermostat.Value)
                {
                    int setPoint = 0;
                    if (!int.TryParse(element.ThermostatSetPoint, out setPoint))
                    {
                        setPoint = 70;
                    }
                    ThermostatDevice.Mode thermostatMode = ThermostatDevice.Mode.Cooling;
                    if (element.ThermostatMode == "cool")
                        thermostatMode = ThermostatDevice.Mode.Cooling;
                    else
                        thermostatMode = ThermostatDevice.Mode.Heating;

                    dev = new ThermostatDevice(deviceName, deviceAddress, thermostatMode, setPoint);
                }
                else
                    dev = new RelayDevice(deviceName, deviceAddress);

                dev.DefaultOffMinutes = element.DefaultOffMinutes;
                dev.IsOnAtSunset = element.IsOnAtSunset ?? false;
                dev.IsOffAtSunrise = element.IsOffAtSunrise ?? false;
                dev.Floor = element.Floor;

                // load random schedule settings
                dev.IsRandomOn = element.IsRandomOn ?? false;
                dev.RandomOnStart = element.RandomStartTime;
                dev.RandomRunDuration = element.RandomRunDuration;
                dev.RandomDurationMin = element.RandomDurationMin;
                dev.RandomDurationMax = element.RandomDurationMax;
                dev.RandomOnChance = element.RandomOnChance;

                _allDevices.Add(deviceAddress.ToString(), dev);
            }

            // iterate through the list once more for slave devices
            foreach (SlapsteonDeviceConfigurationElement element in slapsteonConfiguration.SlapsteonDevices)
            {
                if (string.IsNullOrEmpty(element.SlaveDevices))
                    continue;

                byte address1 = StringToByte(element.Address.Substring(0, 2));
                byte address2 = StringToByte(element.Address.Substring(2, 2));
                byte address3 = StringToByte(element.Address.Substring(4, 2));

                DeviceAddress deviceAddress = new DeviceAddress(address1, address2, address3);

                string[] slaveDeviceList = element.SlaveDevices.Split(',', '|', ' ');
                List<Device> slaveDevices = new List<Device>();
                foreach (string slaveDeviceName in slaveDeviceList)
                {
                    Device slaveDevice = _allDevices.Values.FirstOrDefault(d => d.Name.ToUpper() == slaveDeviceName.ToUpper());

                    if (null == slaveDevice)
                        continue;
                    slaveDevice.IsSlaveDevice = true;

                    slaveDevices.Add(slaveDevice);
                }


                _allDevices[deviceAddress.ToString()].SlaveDevices = slaveDevices;
                log.Debug(string.Format("added {0} slave device(s) for device {1}", slaveDevices.Count, element.Name));
            }

            // deserialize the DeviceALDB
            DeserializeStoredDeviceALDB();



            GetStatusForAllDevices();
            Thread.Sleep(300);
            GetALDBForAllDevices();

            if (saveALDB)
                SerializeDeviceALDB();

            
        }

        private byte[] _lastSentCommand;

        //public void ParseCommand(byte[] commandBytes)
        //{
        //    //if (commandBytes.Length < 5)
        //    //{
        //    //    log.Error(string.Format("Received command not of sufficient length ({0}), command: {1}", commandBytes.Length, BytesToString(commandBytes)));
        //    //    return;
        //    //}

        //    if (commandBytes[0] != 0x02)
        //    {
        //        log.Error(string.Format("Received Command must start with 0x02.  Received command: {0}", BytesToString(commandBytes)));
        //        return;
        //    }

        //    // determine whether the command is an ack
        //    if (CompareToLastSentCommand(commandBytes))
        //    {
        //        // no need to parse ACK commands
        //        if (commandBytes[_lastSentCommand.Length] == 0x06)
        //        {
        //            if (commandBytes.Length > _lastSentCommand.Length + 1)
        //            {
        //                byte[] remainingBytes = new byte[commandBytes.Length - (_lastSentCommand.Length + 1)];
        //                Buffer.BlockCopy(commandBytes, _lastSentCommand.Length + 1, remainingBytes, 0, remainingBytes.Length);
        //                ParseCommand(remainingBytes);

        //            }

        //            _lastSentCommand = null;
        //            return;
        //        }
        //        else if (commandBytes[_lastSentCommand.Length] == 0x15)
        //        {
        //            log.Warn(string.Format("Received NAK for Command {0}", BytesToString(commandBytes)));
        //            _lastSentCommand = null;
        //            return;
        //        }
        //    }

        //    if (0x50 == commandBytes[1]) // STD message received (11 bytes)
        //    {
        //        if (commandBytes.Length < 11)
        //        {
        //            log.Error(string.Format("Bad standard command received {0}", BytesToString(commandBytes)));
        //            return;
        //        }

        //        byte[] singleCommand = new byte[11];
        //        for (int i = 0; i < 11; i++)
        //        {
        //            singleCommand[i]=commandBytes[i];

        //        }

        //        ProcessStandardReceivedMessage(singleCommand);

        //        if (commandBytes.Length > 11)
        //        {
        //            byte[] remainingBytes = new byte[commandBytes.Length - 11];
        //            Buffer.BlockCopy(commandBytes, 11, remainingBytes, 0, commandBytes.Length - 11);
        //            ParseCommand(remainingBytes);
        //        }
        //    }
        //    else if (0x51 == commandBytes[1]) // EXT message received 
        //    {
        //        if (commandBytes.Length < 25)
        //        {
        //            log.Error(string.Format("Bad extended command received {0}", BytesToString(commandBytes)));
        //            return;
        //        }

        //        byte[] singleCommand = new byte[25];
        //        for (int i = 0; i < 25; i++)
        //        {
        //            singleCommand[i]=commandBytes[i];
        //        }
        //    }
        //}

        //public void SetLastCommand(byte[] cmd)
        //{
        //    _lastSentCommand = cmd;
        //}

        private void ProcessStandardReceivedMessage(byte[] message)
        {
            DeviceAddress fromAddress = new DeviceAddress(message[2],message[3],message[4]);
            DeviceAddress toAddress = new DeviceAddress(message[5], message[6], message[7]);

            // check flags for group/device
            byte flagByte = (byte)(message[8] & (byte)0xE0);
            MessageFlag flag = (MessageFlag)flagByte;
            string flagDescription = "unknown";

            switch (flag)
            {
                case MessageFlag.DirectMessage:
                    flagDescription = "Direct Message";
                    break;
                case MessageFlag.ACKDirectMessage:
                    flagDescription = "ACK Direct Message";
                    break;
                case MessageFlag.ACKGroupCleanupDirectMessage:
                    flagDescription = "ACK Group Cleanup DM";
                    break;
                case MessageFlag.BroadcastMessage:
                    flagDescription = "Broadcast Message";
                    break;
                case MessageFlag.GroupBroadcastMessage:
                    flagDescription = "Group Broadcast Message";
                    break;
                case MessageFlag.GroupCleanupDirectMessage:
                    flagDescription = "Group Cleanup DM";
                    break;
                case MessageFlag.NAKDirectMessage:
                    flagDescription = "NAK Direct Message";
                    break;
                case MessageFlag.NAKGroupCleanupDirectMessage:
                    flagDescription = "NAK Group Cleanup DM";
                    break;
                default:
                    break;
            }

            if ((message[8] & 0x10) != 0) // standard message
            { 
                log.Error(string.Format("Process Standard Received Message got extended message flag"));
                return;
            }

            int retransmissions = (message[8] & 0x0C) >> 2;

            int maxHops = message[8] & 0x03;
            
            byte command1 = message[9];
            byte command2=message[10];

            string commandType = "unknown";
            Device sourceDevice = FindDeviceForAddress(fromAddress.ToString());
            Device targetDevice = FindDeviceForAddress(toAddress.ToString());
            if (null != targetDevice && targetDevice is PLMDevice)
            {
                if (_gettingStatus)
                {
                    byte delta = 0x00;
                    //if (_gettingOpFlags)
                    //{
                    //    delta = message[10];
                    //    sourceDevice.Delta = delta;
                    //    log.InfoFormat("Attempting to read database delta from ops flags, result: {0}", delta);
                    //}
                    //else
                    //{
                        // read the command2 and 1 of the response to get the level as well

                    byte level = message[10];
                    delta = message[9];

                    sourceDevice.Status = ((int)level / 255m) * 100m;
                    sourceDevice.Delta = delta;

                    //}
                    _statusEventWaitHandle.Set();
                }
                else if (sourceDevice is ThermostatDevice)
                {
                    if (_gettingThermostat)
                    {
                        if (command1 == 0x6A)
                        {
                            if (_gettingTemp)
                            {
                                ((ThermostatDevice)sourceDevice).AmbientTemperature = (int)Math.Round(((decimal)command2) / 2m,0);
                                _gettingTemp = false;
                            }
                            else if (_gettingSetPoint)
                            {
                                if (((ThermostatDevice)sourceDevice).CurrentMode == ThermostatDevice.Mode.Cooling)
                                    ((ThermostatDevice)sourceDevice).CoolSetPoint = (int)Math.Round(((decimal)command2) / 2m, 0);
                                else
                                    ((ThermostatDevice)sourceDevice).HeatSetPoint = (int)Math.Round(((decimal)command2) / 2m, 0);

                                _gettingSetPoint = false;

                            }
                            else if (_gettingHumidity)
                            {
                                ((ThermostatDevice)sourceDevice).Humidity = (int)command2;

                                _gettingHumidity = false;

                            }
                            else
                                log.WarnFormat("Received thermostat update {0} and unsure of what we requested.", command2.ToString("X"));

                            _statusEventWaitHandle.Set();

                        }
                        return;
                    }
                                   
                    ThermostatDevice thermostat = sourceDevice as ThermostatDevice;

                    switch (command1)
                    {
                        case Constants.STD_COMMAND_THERMOSTAT_STATUS_TEMP:
                            thermostat.AmbientTemperature = (int)Math.Round(((decimal)command2) * 0.5m, 0);

                            log.InfoFormat("Received temperature update: {0}", thermostat.AmbientTemperature);
                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(thermostat.Name,
                                string.Format("Temperature detected: {0}°F", thermostat.AmbientTemperature)));

                            break;
                        case Constants.STD_COMMAND_THERMOSTAT_STATUS_HUMIDITY:
                            thermostat.Humidity = (int)command2;
                            log.InfoFormat("Received humidity update: {0}", thermostat.Humidity);

                            break;
                        case Constants.STD_COMMAND_THERMOSTAT_STATUS_MODE_FAN:
                            log.InfoFormat("Thermostat mode byte: {0}", command2.ToString("X"));
                            // extract mode, low byte 0=off,1=heat,2=cool,3=auto,4=program
                            if ((command2 & 0x0F) == 0x00)
                                thermostat.CurrentMode = ThermostatDevice.Mode.Off;
                            else if ((command2 & 0x0F) == 0x01)
                                thermostat.CurrentMode = ThermostatDevice.Mode.Heating;
                            else if ((command2 & 0x0F) == 0x02)
                                thermostat.CurrentMode = ThermostatDevice.Mode.Cooling;
                            else if ((command2 & 0x0F) == 0x03)
                                thermostat.CurrentMode = ThermostatDevice.Mode.Auto;
                            else if ((command2 & 0x0F) == 0x04)
                                thermostat.CurrentMode = ThermostatDevice.Mode.Program;
                            else
                                log.WarnFormat("Unable to determine mode from status byte: {0}", command2.ToString("X"));

                            // extract fan info, high byte command 2, 0=auto,1=alwayson
                            if ((command2 & 0xF0) == 0x00)
                                thermostat.Fan = ThermostatDevice.FanMode.Auto;
                            else if ((command2 & 0xF0) == 0x10)
                                thermostat.Fan = ThermostatDevice.FanMode.AlwaysOn;
                            else
                                log.WarnFormat("Unable to determine fan from status byte: {0}", command2.ToString("X"));

                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(thermostat.Name,
                                string.Format("Mode: {0}, Fan: {1}", thermostat.CurrentMode, thermostat.Fan)));
                            break;
                        case Constants.STD_COMMAND_THERMOSTAT_STATUS_COOL_SET:
                            thermostat.CoolSetPoint = (int)(command2);
                            log.InfoFormat("Received thermostat cool setpoint update: {0}", thermostat.CoolSetPoint);

                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(thermostat.Name,
                                string.Format("Cool set point: {0}", thermostat.CoolSetPoint)));

                            break;
                        case Constants.STD_COMMAND_THERMOSTAT_STATUS_HEAT_SET:
                            thermostat.HeatSetPoint = (int)(command2);
                            log.InfoFormat("Received thermostat heat setpoint update: {0}", thermostat.HeatSetPoint);
                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(thermostat.Name,
                                string.Format("Heat set point: {0}", thermostat.HeatSetPoint)));
                            break;
                        default:
                            log.InfoFormat("Unknown message from thermostat device received: {0}", command1.ToString("X"));
                            break;
                    }
                }

                // this is received before the ALDB comes, its part of a sequence of extended messages
                // but 0x2F as a standard message is LightOffAtRampRate
                if (command1 == 0x2F)
                {
                    log.Info(string.Format("Received Message from {0} to {1} of type: ({2}):{3} ({4})", GetDeviceName(fromAddress.ToString()), GetDeviceName(toAddress.ToString()), command1.ToString("X"), command2.ToString("X"), flagDescription));
                    log.Info("Beginning of ALDB records.");
                    return;
                } // same as above but for reading device info
                else if (command1 == 0x2E)
                {
                    log.Info(string.Format("Received Message from {0} to {1} of type: ({2}):{3} ({4})", GetDeviceName(fromAddress.ToString()), GetDeviceName(toAddress.ToString()), command1.ToString("X"), command2.ToString("X"), flagDescription));
                    log.Info("Beginning of Extended Read");
                    return;
                }
            }

            switch (command1)
            {
                case Constants.STD_COMMAND_BEEP:
                    break;
                case Constants.STD_COMMAND_BRIGHT:
                    break;
                case Constants.STD_COMMAND_DIM:
                    commandType = "Dim";
                    break;
                case Constants.STD_COMMAND_FAST_OFF:
                    if (null != sourceDevice && (flag & MessageFlag.ACKDirectMessage) == 0)
                    {
                        if ((null != targetDevice && targetDevice is PLMDevice) || toAddress.Byte3 == 0x01)
                        {
                            sourceDevice.Status = 0;
                            sourceDevice.LastOff = DateTime.Now;
                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(sourceDevice.Name,
                                string.Format("Fast off by keypress.")));
                        }
                    }

                    commandType = "FastOff";
                    break;
                case Constants.STD_COMMAND_FAST_ON:
                    if (null != sourceDevice && (flag & MessageFlag.ACKDirectMessage) == 0)
                    {
                        if ((null != targetDevice && targetDevice is PLMDevice) || toAddress.Byte3 == 0x01)
                        {
                            sourceDevice.LastOn = DateTime.Now;
                            if (sourceDevice is DimmerDevice)
                                sourceDevice.Status = (command2 * 100) / 0xFF;
                            else
                                sourceDevice.Status = 100;
                            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(sourceDevice.Name,
                                string.Format("Fast on by keypress.")));
                        }
                    }

                    // setting a timer on an ack message is redundant
                    if (sourceDevice.DefaultOffMinutes.HasValue && ((flag & MessageFlag.ACKDirectMessage) != MessageFlag.ACKDirectMessage))
                    {
                        log.DebugFormat("Setting default off timer for device: {0} minutes.", sourceDevice.DefaultOffMinutes.Value);
                        sourceDevice.SetTimer(new Devices.DeviceTimerCallBack(DeviceTimerCallBack));
                    }

                    commandType = "FastOn";
                    break;
                case Constants.STD_COMMAND_GET_OP_FLAGS:
                    break;
                case Constants.STD_COMMAND_LIGHT_INSTANT_CHANGE:
                    break;
                case Constants.STD_COMMAND_LIGHT_MANUAL_OFF:
                    commandType = "LightManualOff";
                    break;
                case Constants.STD_COMMAND_LIGHT_MANUAL_ON:
                    commandType = "LightManualOn";
                    break;
                case Constants.STD_COMMAND_LIGHT_RAMP_OFF:
                    if (null != sourceDevice && (flag & MessageFlag.ACKDirectMessage) == 0)
                    {
                        if ((null != targetDevice && targetDevice is PLMDevice) || toAddress.Byte3 == 0x01)
                        {
                            sourceDevice.Status = 0;
                            sourceDevice.LastOff = DateTime.Now;
                        }
                    }

                    commandType = "LightRampOff";
                    break;
                case Constants.STD_COMMAND_LIGHT_RAMP_ON:
                    if (null != sourceDevice && (flag & MessageFlag.ACKDirectMessage) == 0)
                    {
                        if ((null != targetDevice && targetDevice is PLMDevice) || toAddress.Byte3 == 0x01)
                        {
                            sourceDevice.LastOn = DateTime.Now;
                            if (sourceDevice is DimmerDevice)
                                sourceDevice.Status = (command2 * 100) / 0xFF;
                            else
                                sourceDevice.Status = 100;
                        }
                    }

                    if (sourceDevice.DefaultOffMinutes.HasValue)
                    {
                        log.DebugFormat("Setting default off timer for device: {0} minutes.", sourceDevice.DefaultOffMinutes.Value);
                        sourceDevice.SetTimer(new DeviceTimerCallBack(DeviceTimerCallBack));
                    }


                    commandType = "LightRampOn";
                    break;

                case Constants.STD_COMMAND_LIGHT_SET_STATUS:
                    commandType = "LightSetStatus";
                    break;

                case Constants.STD_COMMAND_OFF:
                    if (null != sourceDevice && (flag & MessageFlag.ACKDirectMessage) == 0)
                    {
                        if ((null != targetDevice && targetDevice is PLMDevice) || toAddress.Byte3 == 0x01)
                        {
                            sourceDevice.Status = 0;
                            sourceDevice.LastOff = DateTime.Now;

                            if (!(sourceDevice is ThermostatDevice))
                            {
                                SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(sourceDevice.Name,
                                      string.Format("Turned off by keypress.")));
                            }
                        }
                    }
                    commandType = "CommandOff";
                    break;
                case Constants.STD_COMMAND_ON:

                    if (null != sourceDevice && (flag & MessageFlag.ACKDirectMessage) == 0)
                    {
                        if ((null != targetDevice && targetDevice is PLMDevice) || toAddress.Byte3 == 0x01)
                        {
                            sourceDevice.LastOn = DateTime.Now;
                            if (sourceDevice is DimmerDevice)
                                sourceDevice.Status = (command2 * 100) / 0xFF;
                            else if (sourceDevice is SensorDevice)
                                sourceDevice.Status = 0;// dont actually turn on
                            else
                                sourceDevice.Status = 100;

                            if (!(sourceDevice is ThermostatDevice))
                            {
                                if (sourceDevice is SensorDevice)
                                    SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(sourceDevice.Name,
                                        string.Format("Motion Detected")));
                                else
                                    SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(sourceDevice.Name,
                                        string.Format("Turned on by keypress.")));
                            }
                        }
                    }

                    if (sourceDevice.DefaultOffMinutes.HasValue)
                    {
                        log.DebugFormat("Setting default off timer for device: {0} minutes.", sourceDevice.DefaultOffMinutes.Value);
                        sourceDevice.SetTimer(new DeviceTimerCallBack(DeviceTimerCallBack));
                    }

                    if (sourceDevice.Name == "zone1IOLinc")
                    {
                        if (null != PartyDetected)
                        {
                            PartyDetected(this);
                        }
                    }

                    commandType = "CommandOn";
                    break;
                case Constants.STD_COMMAND_REMOTE_SET_BUTTON_TAP:
                    break;
                case Constants.STD_COMMAND_SET_OP_FLAGS:
                    break;
                case Constants.STD_COMMAND_START_MANUAL:
                    commandType = "StartManual";
                    break;
                case Constants.STD_COMMAND_STATUS_REQUEST:
                    break;
                case Constants.STD_COMMAND_STOP_MANUAL:
                    commandType = "StopManual";
                    break;
                default:
                    break;
            }

            ProcessRelatedDeviceEvents(command1, command2, fromAddress, toAddress);

            log.Info(string.Format("Received Message from {0} to {1} of type: {2}({3}):{4} ({5})", GetDeviceName(fromAddress.ToString()), GetDeviceName(toAddress.ToString()), commandType, command1.ToString("X"), command2.ToString("X"), flagDescription));

            if (null != InsteonTrafficDetected)
            {
                InsteonTrafficEventArgs args = new InsteonTrafficEventArgs();
                args.Source = _allDevices.ContainsKey(fromAddress.ToString()) ? _allDevices[fromAddress.ToString()] : null;
                args.Destination = toAddress;
                args.Flags = flag;
                args.Command1 = command1;
                args.Command2 = command2;
                args.Description = string.Format("Command={0}/DestName={1}", commandType, GetDeviceName(toAddress.ToString()));
                InsteonTrafficDetected(this, args);
            }
        }

        private void ProcessExtendedMessageReceived(byte[] message)
        {
            switch (message[9])
            {
                case Constants.EXT_COMMAND_READ_WRITE_ALDB:
                    ProcessALDBResponse(message);
                    return;
                case Constants.EXT_COMMAND_EXTENDED_GET_SET:
                    ProcessExtendedGetResponse(message);
                    return;
                default:
                    break;
            }

        }

        private void ProcessExtendedGetResponse(byte[] message)
        {

            try
            {
                byte deviceAddress1 = message[2];
                byte deviceAddress2 = message[3];
                byte deviceAddress3 = message[4];

                DeviceAddress deviceAddress = new DeviceAddress(deviceAddress1, deviceAddress2, deviceAddress3);

                Device device = _allDevices[deviceAddress.ToString()];
                if (device is IMultiButtonDevice)
                {

                    IMultiButtonDevice multiButtonDevice= device as IMultiButtonDevice;

                    // for now just get the button mask...
                    multiButtonDevice.KPLButtonMask = message[21];

                    if (_gettingExtendedStatus)
                        _extendedGetWaitHandle.Set();
                }
                else if (device is ThermostatDevice){
                    ThermostatDevice thermostatDevice = device as ThermostatDevice;
                    // message[11] is data 1
                    // message[13] and 11 would be local temp high and low
                    // message[15] = data5 = humidity
                    // message[16] = data6 = tempoffset
                    // message[17] = data7 = humidity offset
                    byte systemMode = message[18];
                    byte fanMode = message[19];

                    if (fanMode == 0x00)
                    {
                        thermostatDevice.Fan = ThermostatDevice.FanMode.Auto;
                    }
                    else thermostatDevice.Fan = ThermostatDevice.FanMode.AlwaysOn;

                    if (systemMode == 0x00)
                        thermostatDevice.CurrentMode = ThermostatDevice.Mode.Off;
                    else if (systemMode == 0x01)
                        thermostatDevice.CurrentMode = ThermostatDevice.Mode.Auto;
                    else if (systemMode == 0x02)
                        thermostatDevice.CurrentMode = ThermostatDevice.Mode.Heating;
                    else if (systemMode == 0x03)
                        thermostatDevice.CurrentMode = ThermostatDevice.Mode.Cooling;
                    else
                        thermostatDevice.CurrentMode = ThermostatDevice.Mode.Program;

                    if (_gettingThermostatExtended)
                        _extendedGetWaitHandle.Set();
                    
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error processing Extended Get Response");
            }

        }

        private void ProcessALDBResponse(byte[] message)
        {
            try
            {
                _lastALDBRecordTime = DateTime.Now;

                byte deviceAddress1 = message[2];
                byte deviceAddress2 = message[3];
                byte deviceAddress3 = message[4];

                DeviceAddress deviceAddress = new DeviceAddress(deviceAddress1, deviceAddress2, deviceAddress3);
                Device device = _allDevices[deviceAddress.ToString()];
                
               
                DeviceALDB deviceALDB = null;
                // record isnt in out ALDB Library yet
                if (!_aldbLibrary.Devices.Exists(a => a.DeviceAddress == deviceAddress.ToString()))
                {
                    deviceALDB = new DeviceALDB();
                    deviceALDB.DeviceAddress = deviceAddress.ToString();
                    deviceALDB.Delta = device.Delta;

                    _aldbLibrary.Devices.Add(deviceALDB);
                }
                else
                    deviceALDB = _aldbLibrary.Devices.First(a => a.DeviceAddress == deviceAddress.ToString());


                // redundant...we should be doing this before we even pull the ALDB
                //if (device.Delta != device.DeviceALDB.Delta)
                //    log.DebugFormat("Device Delta {0} did not match the stored version {1}", device.Delta.ToString("X"), device.DeviceALDB.Delta.ToString("X"));


                ALDBRecord record = new ALDBRecord();
                record.AddressMSB = message[13];
                record.AddressLSB = message[14];
                record.Flags = message[16];
                record.Group = message[17];
                record.Address1 = message[18];
                record.Address2 = message[19];
                record.Address3 = message[20];
                record.LocalData1 = message[21];
                record.LocalData2 = message[22];
                record.LocalData3 = message[23];

                if ((record.Flags == 0x00) && ((record.Address1 == 0x00) && (record.Address2 == 0x00) && (record.Address3 == 0x00)))
                {
                    _aldbFinishedForDevice = true;
                    log.Info("Reached last address record.");
                    _aldbEventWaitHandle.Set();

                    return;
                }
                deviceALDB.ALDBRecords.Add(record);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error processing ALDB Response");
            }
        }

        public void ProcessSendingRelatedEvents(byte command1, Device sourceDevice)
        {
            log.InfoFormat("Processing related device events for 0x{0} for device {1}", command1.ToString("X"), sourceDevice.Name);

            // if we're turning a light on via API, arm any timers, update KPL buttons, etc
            foreach (string i in _allDevices.Keys)
            {
                // only really care about updating KPL buttons
                if (!(_allDevices[i] is IMultiButtonDevice)) continue;

                IMultiButtonDevice kplDevice = _allDevices[i] as IMultiButtonDevice;

                // ignore this change if it will be triggered by a slave device command
                if (sourceDevice.SlaveDevices.Contains(_allDevices[i])) continue;
                
                DeviceALDB devALDB = _aldbLibrary.Devices.FirstOrDefault(d => d.DeviceAddress == _allDevices[i].Address.ToString());

                if (null == devALDB)
                    continue;

                // go thru each device looking for responders
                foreach (ALDBRecord record in devALDB.ALDBRecords)
                {
                    // record not active
                    if ((record.Flags & 0x80) == 0)
                        continue;

                    if ((record.Flags & 0x40) == 0 && // responder
                            record.AddressToString() == sourceDevice.Address.ToString())
                    {
                        // only need to worry about group 1 since this is sent from single group devices...
                        if (record.Group != 0x01) continue;

                        // set timers
                        if (_allDevices[i].DefaultOffMinutes.HasValue && (command1 == Constants.STD_COMMAND_FAST_ON || command1 == Constants.STD_COMMAND_ON))
                        {
                            log.InfoFormat("Setting off timer for responder {0} for {1} minutes", _allDevices[i].Name, _allDevices[i].DefaultOffMinutes.Value);
                            _allDevices[i].SetTimer(new DeviceTimerCallBack(DeviceTimerCallBack));
                        }

                        // local data says what button to turn off
                        byte flipMask = 0xFF;
                        switch (record.LocalData3) 
                        {
                            case 0x03:
                                flipMask = 0x04;
                                break;
                            case 0x04:
                                flipMask = 0x08;
                                break;
                            case 0x05:
                                flipMask = 0x10;
                                break;
                            case 0x06:
                                flipMask = 0x20;
                                break;
                            default:
                                break;
                        }

                        if (command1 == Constants.STD_COMMAND_ON || command1 == Constants.STD_COMMAND_FAST_ON)
                        {
                            kplDevice.KPLButtonMask |= flipMask;
                        }
                        else if (command1 == Constants.STD_COMMAND_OFF || command1 == Constants.STD_COMMAND_FAST_OFF)
                        {
                            kplDevice.KPLButtonMask &= (byte)(0xFF ^ flipMask);
                        }
                        log.InfoFormat("Updated Button Mask to {0} for device {1}", kplDevice.KPLButtonMask.ToString("X"), _allDevices[i].Name);

                        Thread.Sleep(500);
                        SendExtendedCommand(_allDevices[i].Address, Constants.EXT_COMMAND_EXTENDED_GET_SET, 0x00, 0x1F, false, record.LocalData2, 0x09, kplDevice.KPLButtonMask, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                }
            }
        }

        private void ProcessRelatedDeviceEvents(byte command1, byte command2, DeviceAddress fromAddress, DeviceAddress toAddress)
        {
            Device sourceDevice = FindDeviceForAddress(fromAddress.ToString());
            Device targetDevice = FindDeviceForAddress(toAddress.ToString());

            // if the target is not an existing device (group target)
            // look at address entries for devices that are RESPONDERS to the 
            // address of the command, matching the group in the address..
            if (null == targetDevice || targetDevice is PLMDevice)
            {
                foreach (string i in _allDevices.Keys)
                {
                    DeviceALDB devALDB = _aldbLibrary.Devices.FirstOrDefault(d => d.DeviceAddress == _allDevices[i].Address.ToString());
                    if (null == devALDB)
                        continue;

                    foreach (ALDBRecord record in devALDB.ALDBRecords)
                    {
                        // record not active
                        if ((record.Flags & 0x80) == 0)
                            continue;

                        if ((record.Flags & 0x40) == 0 && // responder
                            record.AddressToString() == fromAddress.ToString())
                        {
                         //   log.DebugFormat("Device: {0}, TargetAddress:{1}, Command2: {2}", _allDevices[i].Name, toAddress.ToString(), command2.ToString("X"));

                            // its possible a kpl button sends a direct message to the PLM and the group # is in cmd2
                            if (record.Group == toAddress.Byte3 || ((null != targetDevice) && ((targetDevice is PLMDevice) && record.Group == command2)))
                            {
                                log.Info(string.Format("Event detected matched ALDB record for device {0}, group {1}.  Local Data: {2} {3} {4}", _allDevices[i].Name, record.Group, record.LocalData1.ToString("X"), record.LocalData2.ToString("X"), record.LocalData3.ToString("X")));

                                if (_allDevices[i] is IMultiButtonDevice)
                                {
                                    IMultiButtonDevice kplDevice = _allDevices[i] as IMultiButtonDevice;
                                    // update KPL button for the action.. mainly concerned with bits --XXXX--
                                    byte flipMask = 0xFF;
                                    switch (record.LocalData3)
                                    {
                                        case 3:
                                            flipMask = 0x04;
                                            break;
                                        case 4:
                                            flipMask = 0x08;
                                            break;
                                        case 5:
                                            flipMask = 0x10;
                                            break;
                                        case 6:
                                            flipMask = 0x20;
                                            break;
                                        default:
                                            break;
                                    }

                                    if (command1 == Constants.STD_COMMAND_ON || command1 == Constants.STD_COMMAND_FAST_ON)
                                    {
                                        kplDevice.KPLButtonMask |= flipMask;
                                    }
                                    else if (command1 == Constants.STD_COMMAND_OFF || command1 == Constants.STD_COMMAND_FAST_OFF)
                                    {
                                        kplDevice.KPLButtonMask &= (byte)(0xFF ^ flipMask);
                                    }
                                    log.InfoFormat("Updated Button Mask to {0} for device {1}", kplDevice.KPLButtonMask.ToString("X"), _allDevices[i].Name);
                                }

                                if ((command1 == Constants.STD_COMMAND_FAST_ON || command1 == Constants.STD_COMMAND_ON) && _allDevices[i].DefaultOffMinutes.HasValue)
                                    _allDevices[i].SetTimer(new DeviceTimerCallBack(DeviceTimerCallBack));

                            }
                        }
                    }
                }
            }
        }

        private bool CompareToLastSentCommand(byte[] command)
        {
            if (null == _lastSentCommand)
                return false;

            if (command.Length < (_lastSentCommand.Length + 1))
                return false;

            for (int i = 0; i < _lastSentCommand.Length; i++)
            {
                if (command[i] != _lastSentCommand[i])
                    return false;
            }

            return true;
        }

        public string SendStandardCommand(DeviceAddress deviceAddress, byte command1, byte command2, byte flags)
        {
            return SendStandardCommand(deviceAddress, command1, command2, flags, false);
        }

        public string SendStandardCommand(DeviceAddress deviceAddress, byte command1, byte command2, byte flags, bool isUnknownDevice)
        {
            string results = null;

            Device targetDevice = null;
            if (!isUnknownDevice && !_allDevices.TryGetValue(deviceAddress.ToString(), out targetDevice))
                throw new Exception("No known device matched the specified address.");

            byte[] command = new byte[8];


            try
            {

                command[0] = 0x02; // Insteon start byte
                command[1] = 0x62; // Standard Command
                command[2] = deviceAddress.Byte1;
                command[3] = deviceAddress.Byte2;
                command[4] = deviceAddress.Byte3;
                command[5] = flags; // for standard 0x0F is good
                command[6] = command1;
                command[7] = command2;

                _plm.Write(command, 0, 8);

                log.Debug(string.Format("Sent Standard Command {0} to device {1} (address: {2})", command1.ToString("X"), null != targetDevice ? targetDevice.Name : "Unknown", null != targetDevice ? targetDevice.AddressString : deviceAddress.ToString()));

                if (null == targetDevice)
                    return null;

                // write more to the command for slave devices
                foreach (Device slaveDevice in targetDevice.SlaveDevices)
                {
                    Thread.Sleep(250);
                    log.Debug(string.Format("Sending duplicate command {0} for slave device {1} (address: {2})", command1.ToString("X"), slaveDevice.Name, slaveDevice.AddressString));
                    command = new byte[8];

                    command[0] = 0x02; // Insteon start byte
                    command[1] = 0x62; // Standard Command
                    command[2] = slaveDevice.Address.Byte1;
                    command[3] = slaveDevice.Address.Byte2;
                    command[4] = slaveDevice.Address.Byte3;
                    command[5] = flags; // for standard 0x0F is good
                    command[6] = command1;
                    command[7] = command2;

                    _plm.Write(command, 0, 8);
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error sending command {0} to {1}. (Flags: {2})", command1.ToString("X"), null != targetDevice ? targetDevice.Name : deviceAddress.ToString(), flags.ToString("X")), ex);
            }

            return results;
        }

        public void SendExtendedCommand(DeviceAddress deviceAddress, byte command1, byte command2, byte flags, bool checksum, params byte[] userData)
        {
            SendExtendedCommand(deviceAddress, command1, command2, flags,checksum, false, userData);
        }

        public void SendExtendedCommand(DeviceAddress deviceAddress, byte command1, byte command2, byte flags, bool checksum, bool isUnknownDevice, params byte[] userData)
        {
            byte[] command = new byte[22];

            Device targetDevice = null;
            if (!isUnknownDevice && !_allDevices.TryGetValue(deviceAddress.ToString(), out targetDevice))
                throw new Exception("No known device matched the specified address.");

            if (userData.Length > 14)
                throw new Exception("Invalid userdata specified");

            try
            {
                lock (this)
                {
                    int total = command1 + command2;

                    command[0] = 0x02; // Insteon start byte
                    command[1] = 0x62; // Standard Command
                    command[2] = deviceAddress.Byte1;
                    command[3] = deviceAddress.Byte2;
                    command[4] = deviceAddress.Byte3;
                    command[5] = flags |= 0x10; // for standard 0x0F is good
                    command[6] = command1;
                    command[7] = command2;
                    if (null != userData)
                    {
                        for (int i = 0; i < userData.Length; i++)
                        {
                            command[8 + i] = userData[i];
                            total += userData[i];
                        }
                    }
                    if (checksum)
                        command[21] = (byte)((total ^ 0xFF) + 1);
                    
                    _plm.Write(command, 0, 22);
                    Thread.Sleep(250);
                    log.Info(string.Format("Sent command {0} to device {1}.  (Command2: {2}, Flags: {3})", command1.ToString("X"), null != targetDevice ? targetDevice.Name : "Unknown", command2.ToString("X"), flags.ToString("X")));
                    _lastSentCommand = command;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error sending command {0} to {1}. (Flags: {2})", command1.ToString("X"), null != targetDevice ? targetDevice.Name : "Unknown", flags.ToString("X")), ex);
            }
        }

        public void SendIMCommandReset()
        {

            try
            {
                byte[] command = new byte[2];
                command[0] = 0x02;
                command[1] = 0x67;

                _plm.Write(command, 0, 2);
                Thread.Sleep(20000);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error occurred while resetting the IM: {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void SendStartAllLink()
        {
            try
            {
                byte[] command = new byte[4];
                command[0] = 0x02;
                command[1] = Constants.IM_COMMAND_START_ALL_LINKING;
                command[2] = 0x03; // Link Code: 0x00 Responder, 0x01 Controller, 0x03 Controller when IM initiates, Responder when other device, 0xFF delete the All-Link
                command[3] = 0x00; // group number

                _plm.Write(command, 0, 4);

            }
            catch (Exception ex)
            {

            }
        }

        public void SendCancelAllLink()
        {
            try
            {
                byte[] command = new byte[2];
                command[0] = 0x02;
                command[1] = Constants.IM_COMMAND_CANCEL_ALL_LINKING;
                _plm.Write(command, 0, 2);
            }
            catch (Exception ex)
            {

            }
        }

        public void GetFirstIMLink()
        {
            try
            {
                byte[] command = new byte[2];
                command[0] = 0x02;
                command[1] = Constants.IM_COMMAND_GET_FIRST_ALL_LINK_RECORD;
                _plm.Write(command, 0, 2);
            }
            catch (Exception ex) { }
        }

        public void GetNextIMLink()
        {
            try
            {
                byte[] command = new byte[2];
                command[0] = 0x02;
                command[1] = Constants.IM_COMMAND_GET_NEXT_ALL_LINK_RECORD;
                _plm.Write(command, 0, 2);
            }
            catch (Exception ex)
            {

            }
        }

        public DeviceStatus GetDeviceStatus(DeviceAddress deviceAddress)
        {
            byte[] command = new byte[8];
            DeviceStatus status = new DeviceStatus();
            try
            {
                if (_allDevices[deviceAddress.ToString()] is ThermostatDevice)
                {
                    _gettingThermostat = true;
                    
                    command[0] = 0x02;
                    command[1] = 0x62;
                    command[2] = deviceAddress.Byte1;
                    command[3] = deviceAddress.Byte2;
                    command[4] = deviceAddress.Byte3;
                    command[5] = 0x0F;
                    command[6] = Constants.STD_COMMAND_THERMOSTAT_STATUS;
                    command[7] = 0x00; // temp

                    _gettingTemp = true;
                    _plm.Write(command, 0, 8);

                    _lastSentCommand = command;

                    _statusEventWaitHandle.Reset();
                    log.Info("Calling WaitOne on StatusEventWaitHandle for Thermostat Temp");
                    _statusEventWaitHandle.WaitOne(10000);
                    log.Info("Completed waiting on temp");

                    Thread.Sleep(300);


                    command[0] = 0x02;
                    command[1] = 0x62;
                    command[2] = deviceAddress.Byte1;
                    command[3] = deviceAddress.Byte2;
                    command[4] = deviceAddress.Byte3;
                    command[5] = 0x0F;
                    command[6] = Constants.STD_COMMAND_THERMOSTAT_STATUS;
                    command[7] = 0x20; // set point

                    _gettingSetPoint = true;
                    _plm.Write(command, 0, 8);

                    _lastSentCommand = command;

                    _statusEventWaitHandle.Reset();
                    log.Info("Calling WaitOne on StatusEventWaitHandle for Thermostat set point");
                    _statusEventWaitHandle.WaitOne(10000);
                    log.Info("Completed waiting on set point");

                    Thread.Sleep(300);

                    command[0] = 0x02;
                    command[1] = 0x62;
                    command[2] = deviceAddress.Byte1;
                    command[3] = deviceAddress.Byte2;
                    command[4] = deviceAddress.Byte3;
                    command[5] = 0x0F;
                    command[6] = Constants.STD_COMMAND_THERMOSTAT_STATUS;
                    command[7] = 0x60; // humidity

                    _gettingHumidity = true;
                    _plm.Write(command, 0, 8);

                    _lastSentCommand = command;

                    _statusEventWaitHandle.Reset();
                    log.Info("Calling WaitOne on StatusEventWaitHandle for Thermostat humidity");
                    _statusEventWaitHandle.WaitOne(10000);
                    log.Info("Completed waiting on humidity");

                    
                    Thread.Sleep(300);

                    _gettingThermostat = false;

                    return null;
                }


                // send a get status command
                command[0] = 0x02;
                command[1] = 0x62;
                command[2] = deviceAddress.Byte1;
                command[3] = deviceAddress.Byte2;
                command[4] = deviceAddress.Byte3;
                command[5] = 0x0F;
                command[6] = Constants.STD_COMMAND_STATUS_REQUEST;
                command[7] = 0x00;

                _gettingStatus = true;

                _plm.Write(command, 0, 8);

                _lastSentCommand = command;

                _statusEventWaitHandle.Reset();
                log.Info("Calling WaitOne on StatusEventWaitHandle");
                _statusEventWaitHandle.WaitOne(10000);
                log.Info("Completed waiting device status handle");

                    



                status = new DeviceStatus();
                status.Delta = _allDevices[deviceAddress.ToString()].Delta;
                status.OnLevel = Math.Round(_allDevices[deviceAddress.ToString()].Status,1);

                _gettingStatus = false;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error getting device status for {0}", deviceAddress.ToString()));
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
            }

            return status;
        }

        public void RefreshThermostatDevice(DeviceAddress address)
        {

        }

        private bool SetMonitorMode()
        {
            bool succeeded = false;
            byte[] cmdBytes = new byte[3];

            try
            {
                lock (this)
                {
                    cmdBytes[0] = 0x02;
                    cmdBytes[1] = 0x6B; // Set IM Config
                    cmdBytes[2] = 0x40; // 1000

                    _plm.Write(cmdBytes, 0, 3);
                    //Thread.Sleep(250);

                    //int numberOfBytesToRead = _plm.BytesToRead;

                    //byte[] bytesRead = new byte[numberOfBytesToRead];

                    //_plm.Read(bytesRead, 0, numberOfBytesToRead);

                    //string byteString = BitConverter.ToString(bytesRead);

                    //log.Info(string.Format("SetMonitorMode received: {0}", byteString));
                }
                succeeded = true;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred setting monitor mode", ex);
            }

            return succeeded;
        }

        private byte[] _buffer = new byte[1024];
        private int _bufferOffset = 0;

        private void _plm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            //log.Info("Data detected from serial port.");
            try
            {
                int bytesToRead = _plm.BytesToRead;
                byte[] bytesRead = new byte[bytesToRead];
                _plm.Read(bytesRead, 0, bytesToRead);

                string bytesAsString = BytesToString(bytesRead);
                //log.Info(string.Format("{0} bytes read from buffer: {1}", bytesToRead, bytesAsString));

                lock (this)
                {
                    Buffer.BlockCopy(bytesRead, 0, _buffer, _bufferOffset, bytesToRead);
                    _bufferOffset += bytesToRead;
                }
                AnalyzeSerialBuffer();
            }
            catch (Exception ex)
            {
                log.Error("Error during read bytes.");
                log.Error(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
            }
        }

        private void AnalyzeSerialBuffer()
        {
            lock (this)
            {
                try
                {
                    InsteonStandardMessage message = null;

                    if (_bufferOffset <= 2)
                    {
                        Thread.Sleep(100);
                        return;
                    }

                    if (_buffer[0] != 0x02)
                        throw new Exception("Buffer does not start with 0x02");

                    byte[] command = null;

                    byte commandByte = _buffer[1];

                    switch (commandByte)
                    {
                        case Constants.IM_COMMAND_SET_IM_CONFIGURATION:
                            if (_bufferOffset < 4)
                                return;
                            command = ExtractCommandFromBuffer(4);

                            break;
                        case Constants.IM_COMMAND_SEND_STANDARD_OR_EXTENDED_MSG:
                            // ack or nak
                            // determine whether the command sent was extended based on the flag byte
                            // read it out of the buffer
                            // not enough data for flag yet
                            if (_bufferOffset < 6)
                                return;

                            if ((_buffer[5] & 0x10) == 0) // standard
                            {
                                if (_bufferOffset < 9)
                                    return;

                                command = ExtractCommandFromBuffer(9);
                            }
                        else
                            {
                                if (_bufferOffset < 23)
                                    return;

                                command = ExtractCommandFromBuffer(23);
                            }

                            break;
                        case Constants.IM_COMMAND_STANDARD_MESSAGE_RECEIVED:
                            // not enough data yet
                            if (_bufferOffset < 11)
                                return;
                            command = ExtractCommandFromBuffer(11);
                            message = CommandBytesToStandardMessage(command);

                            ProcessStandardReceivedMessage(command);
                            break;
                        case Constants.IM_COMMAND_EXTENDED_MESSAGE_RECEIVED:
                            // not enough data yet
                            if (_bufferOffset < 25)
                                return;
                            command = ExtractCommandFromBuffer(25);
                            message = CommandBytesToExtendedMessage(command);
                            ProcessExtendedMessageReceived(command);
                            break;

                        case Constants.IM_COMMAND_SEND_ALL_LINK_COMMAND:
                            if (_bufferOffset < 6)
                                return;

                            command = ExtractCommandFromBuffer(6);
                            
                            break;
                        case Constants.IM_COMMAND_ALL_LINK_CLEANUP_FAILURE_REPORT:
                            if (_bufferOffset < 7)
                                return;

                            command = ExtractCommandFromBuffer(7);

                            break;
                        case Constants.IM_COMMAND_ALL_LINK_CLEANUP_STATUS_REOPRT:
                            if (_bufferOffset < 3)
                                return;

                            command = ExtractCommandFromBuffer(3);
                            break;
                        case Constants.IM_COMMAND_START_ALL_LINKING:
                            if (_bufferOffset < 5)
                                return;
                            command = ExtractCommandFromBuffer(5);
                            break;
                        case Constants.IM_COMMAND_CANCEL_ALL_LINKING:
                            if (_bufferOffset < 3)
                                return;
                            command = ExtractCommandFromBuffer(3);
                            break;
                        case Constants.IM_COMMAND_ALL_LINKING_COMPLETED:
                            if (_bufferOffset < 10)
                                return;
                            command = ExtractCommandFromBuffer(10);
                            break;
                        case Constants.IM_COMMAND_ALL_LINK_RECORD_RESPONSE:
                            if (_bufferOffset < 10)
                                return;
                            command = ExtractCommandFromBuffer(10);

                            ProcessAllLinkResponse(command);
                            break;
                        case Constants.IM_COMMAND_GET_NEXT_ALL_LINK_RECORD:
                            if (_bufferOffset < 3)
                                return;
                            command = ExtractCommandFromBuffer(3);
                            break;
                        case Constants.IM_COMMAND_RESET_IM:

                            if (_bufferOffset < 3)
                                break;
                            command = ExtractCommandFromBuffer(3);

                            if (command[2] == 0x06)
                                log.Info("Successfully Reset the IM");
                            else
                                log.InfoFormat("Failed to Reset IM: {0}", command[2].ToString("X"));

                            break;

                        case Constants.IM_COMMAND_GET_FIRST_ALL_LINK_RECORD:
                            if (_bufferOffset < 3)
                                break;
                            command = ExtractCommandFromBuffer(3);

                            // if getting first link we want to clear the list of links
                            _imLinks.Clear();
                            break;
                        default:
                            log.Warn(string.Format("Unknown IM command type in the buffer: {0}.  Cleaning the buffer.", _buffer[1].ToString("X")));
                            
                            string bufferString = BytesToString(_buffer);
                            log.Warn(string.Format("Current Buffer Size: {0}.  Contents: {1}", _bufferOffset, bufferString));

                            _buffer = new byte[1024];
                            _bufferOffset = 0;

                            return;
                    }

                    if (null != command)
                    {
                        string commandString = BytesToString(command);
                        log.Debug(string.Format("Received command {0} {1}", commandString, (Constants.IM_COMMAND)commandByte));
                    }
                    if (null != message)
                    {
                        log.DebugFormat("Received message from {0} to {1}. Command: {2}, flags: {3}", message.SourceName, message.TargetName, message.CommandDescription, message.FlagDescription);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error while Analyzing buffer.  Cleaning contents.");
                    string bufferString = BytesToString(_buffer);
                    log.Error(string.Format("Current Buffer Size: {0}.  Contents: {1}", _bufferOffset, bufferString));
                    log.Error(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));

                    _buffer = new byte[1024];
                    _bufferOffset = 0;
                }

                // if there is anything left in the buffer continue to proces
                if (_bufferOffset > 0)
                    AnalyzeSerialBuffer();
            }
        }

        private void GetAddressRecords(DeviceAddress address)
        {
            try
            {

                // send the get address command
                byte[] command = new byte[22];
                command[0] = 0x02;
                command[1] = 0x62; // Standard Command
                command[2] = address.Byte1;
                command[3] = address.Byte2;
                command[4] = address.Byte3;
                command[5] = 0x1F; // for standard 0x0F is good, this is extended
                command[6] = Constants.EXT_COMMAND_READ_WRITE_ALDB;
                command[7] = 0x00;

                _plm.Write(command, 0, 22);

            }
            catch (Exception ex)
            {

            }
        }

        private Device FindDeviceForAddress(string address)
        {
            if (!_allDevices.ContainsKey(address))
                return null;
            return _allDevices[address];
        }

        private string BytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }

        private string GetDeviceName(string address)
        {
            if (!_allDevices.ContainsKey(address))
                return string.Format("unknown({0})", address);
            return _allDevices[address].Name;
        }

        private InsteonStandardMessage CommandBytesToStandardMessage(byte[] command)
        {
            DeviceAddress sourceAddress = new DeviceAddress(command[2], command[3], command[4]);
            DeviceAddress targetAddress =new DeviceAddress(command[5], command[6], command[7]);
            byte messageFlag = command[8];
            InsteonStandardMessage message = new InsteonStandardMessage(sourceAddress, targetAddress, command[9], command[10], messageFlag);

            message.CommandDescription = StdCommandToString(command[9]);
            Device sourceDevice = _allDevices.Values.FirstOrDefault(d => d.Address.StringAddress == sourceAddress.StringAddress);
            if (null != sourceDevice)
                message.SourceName = sourceDevice.Name;
            else
                message.SourceName = sourceAddress.StringAddress;

            Device targetDevice = _allDevices.Values.FirstOrDefault(d => d.Address.StringAddress == targetAddress.StringAddress);
            if (null != targetDevice)
                message.TargetName = targetDevice.Name;
            else
                message.TargetName = targetAddress.StringAddress;

            return message;
        }

        private InsteonExtendedMessage CommandBytesToExtendedMessage(byte[] command) {
            DeviceAddress sourceAddress = new DeviceAddress(command[2], command[3], command[4]);
            DeviceAddress targetAddress = new DeviceAddress(command[5], command[6], command[7]);
            byte messageFlag = command[8];
            byte[] extendedData = new byte[14];
            Array.Copy(command, 11, extendedData, 0, 14);
            InsteonExtendedMessage message = new InsteonExtendedMessage(sourceAddress, targetAddress, command[9], command[10], extendedData, messageFlag);


            message.CommandDescription = ExtCommandToString(command[9]);
            Device sourceDevice = _allDevices.Values.FirstOrDefault(d => d.Address == sourceAddress);
            if (null != sourceDevice)
                message.SourceName = sourceDevice.Name;
            else
                message.SourceName = sourceAddress.StringAddress;

            Device targetDevice = _allDevices.Values.FirstOrDefault(d => d.Address == targetAddress);
            if (null != targetDevice)
                message.TargetName = targetDevice.Name;
            else
                message.TargetName = targetAddress.StringAddress;


            return message;
        }

        private string StdCommandToString(byte command)
        {
            switch (command)
            {
                case 0x01:
                    return "Assign to ALL-Link Group";
                case 0x02:
                    return "Delete from ALL-Link Group";
                case 0x09:
                    return "Enter Linking Mode";
                case 0x0A:
                    return "Enter Unlinking Mode";
                case 0x0D:
                    return "Get INSTEON Engine Version";
                case 0x0F:
                    return "Ping";
                case 0x10:
                    return "ID Request";
                case 0x11:
                    return "On";
                case 0x12:
                    return "Light On Fast";
                case 0x13:
                    return "Off";
                case 0x14:
                    return "Light Off Fast";
                case 0x15:
                    return "Brighten One Step";
                case 0x16:
                    return "Dim One Step";
                case 0x17:
                    return "Light Start Manual Change";
                case 0x18:
                    return "Light Stop Manual Change";
                case 0x19:
                    return "Light Status Request";
                case 0x1F:
                    return "Get Operating Flags";
                case 0x20:
                    return "Set Operating Flags";
                case 0x21:
                    return "Light Instant Change";
                case 0x22:
                    return "Light Manually Turned Off";
                case 0x23:
                    return "Light Manually Turned On";
                case 0x24:
                    return "Reread Init Values";
                case 0x25:
                    return "Remote SET Button Tap";
                case 0x27:
                    return "Light Set Status";
                case 0x28:
                    return "Set Address MSB";
                case 0x29:
                    return "Poke One Byte";
                case 0x2B:
                    return "Peek One Byte";
                case 0x2C:
                    return "Peek One Byte Internal";
                case 0x2D:
                    return "Poke One Byte Internal";
                case 0x2E:
                    return "Light ON at Ramp Rate";
                default:
                    return BitConverter.ToString(new byte[] { command });
            }
        }

        private string ExtCommandToString(byte command)
        {
            switch (command)
            {
                case 0x2A:
                    return "Block Data Transfer";
                case 0x2E:
                    return "Extended Set/Get";
                case 0x2F:
                    return "Read/Write ALL-Link Database";
                case 0x30:
                    return "Trigger ALL-Link Command";
                default:
                    return BitConverter.ToString(new byte[] { command });
            }
        }


        private byte[] ExtractCommandFromBuffer(int size)
        {
            byte[] command = new byte[size];

            Buffer.BlockCopy(_buffer, 0, command, 0, size);

            // reset the buffer
            byte[] temp = new byte[1024];
            Buffer.BlockCopy(_buffer, size, temp, 0, _bufferOffset - size);

            _buffer = temp;
            _bufferOffset -= size;

            return command;

        }

        public void GetALDBForDevice(DeviceAddress address)
        {
            GetAddressRecords(address);

            log.Info("Waiting on ALDB Event Handle");
            _aldbEventWaitHandle.WaitOne(10000);
            log.Info("Finished Waiting on ALDB entry.");
        }

        public void GetALDBForAllDevices()
        {
            if (_aldbLibrary.LastSynchronized > DateTime.Now.AddDays(-7))
            {
                return;
            }

            _aldbLibrary.LastSynchronized = DateTime.Now;
            foreach (string key in _allDevices.Keys)
            {
                // the PLM will not respond normally to this command, there are special commands for it
                // so do not get it in this fashion, also battery devices are asleep so ignore them
                if ((_allDevices[key] is PLMDevice)|| (_allDevices[key] is SensorDevice))
                    continue;

                DeviceALDB targetALDB = _aldbLibrary.Devices.FirstOrDefault(a => a.DeviceAddress == key);
                

                if (null != targetALDB && targetALDB.Delta == _allDevices[key].Delta && targetALDB.Delta != 0x00 && (targetALDB.ALDBRecords != null && targetALDB.ALDBRecords.Count > 0))
                {
                    targetALDB.Name = _allDevices[key].Name;
                    log.DebugFormat("Device {0}'s Delta matched the stored delta ({1})", _allDevices[key].Name, _allDevices[key].Delta.ToString("X"));
                    continue;
                }
                else if (null != targetALDB)
                {
                    targetALDB.Name = _allDevices[key].Name;
                    log.DebugFormat("Device {0}'s Delta ({1}) did not match the stored delta ({2})", _allDevices[key].Name, _allDevices[key].Delta.ToString("X"), targetALDB.Delta.ToString("X"));
                    targetALDB.Delta = _allDevices[key].Delta;
                }
                else
                {
                    log.DebugFormat("Device {0} was not in the ALDB Library.  Added with Delta {1}", _allDevices[key].Name, _allDevices[key].Delta.ToString("X"));
                    targetALDB = new DeviceALDB();
                    targetALDB.Name = _allDevices[key].Name; 
                    targetALDB.Delta = _allDevices[key].Delta;
                    targetALDB.DeviceAddress = _allDevices[key].AddressString;

                    _aldbLibrary.Devices.Add(targetALDB);
                }

                log.Info(string.Format("Getting ALDB records for {0}", _allDevices[key].Name));

                // clear out the existing data from the dictionary so it doesn't get duplicated
                targetALDB.ALDBRecords.Clear();

                GetAddressRecords(_allDevices[key].Address);

                log.Info("Waiting on ALDB Event Handle");
                // todo: make a better way of detecting a timeout..as my database sizes grow i keep incrementing
                // this timeout to prevent quitting before we're done
                _aldbFinishedForDevice = false;
                _lastALDBRecordTime = DateTime.Now;
                while (true)
                {
                    _aldbEventWaitHandle.WaitOne(5000);

                    if (_aldbFinishedForDevice || DateTime.Now.Subtract(_lastALDBRecordTime).TotalSeconds > 20)
                    {
                        log.DebugFormat("Exiting ALDB Loop. Last Record Found: {0}, Last Successful Record Time: {1}", _aldbFinishedForDevice, _lastALDBRecordTime);
                        break;
                    }
                }
                log.Info("Finished Waiting on ALDB entry.");

                log.Info(string.Format("Found {0} ALDB entries for device: {1}", targetALDB.ALDBRecords.Count, _allDevices[key].Name));

            }
        }

        public void GetStatusForAllDevices()
        {
            foreach (string key in _allDevices.Keys)
            {

                Device device = _allDevices[key];

                // the PLM will not respond normally to this command, there are special commands for it
                // so do not get it in this fashion
                if (device is PLMDevice || device is SensorDevice)
                    continue;
                log.Info(string.Format("Getting Status for {0}", _allDevices[key].Name));

                if (device is ThermostatDevice)
                {
                    // getting the extended info first is better for the other information.
                    GetThermostatExtendedInformation(device.Address);
                    Thread.Sleep(200);
                }

                DeviceStatus status = GetDeviceStatus(_allDevices[key].Address);

                if (device is ThermostatDevice)
                {
                    ThermostatDevice thermostat = _allDevices[key] as ThermostatDevice;


                    log.InfoFormat("Status for device: {0} was Temp: {1}, Humidity: {2}, SetPoint: {3}, Mode: {4}, Fan: {5}", thermostat.Name, thermostat.AmbientTemperature, thermostat.Humidity, thermostat.CurrentMode == ThermostatDevice.Mode.Cooling ? thermostat.CoolSetPoint : thermostat.HeatSetPoint, thermostat.CurrentMode, thermostat.Fan);
                    _gettingThermostat = false;
                    continue;
                }

                log.Info(string.Format("Status for device: {0} was {1}", _allDevices[key].Name, (null != status ? status.OnLevel : 0m)));

                device.Status = (null != status ? status.OnLevel : 0m);

                Thread.Sleep(300);

                if (device is IMultiButtonDevice)
                {
                    log.InfoFormat("Getting KPL Button Mask for device {0}", _allDevices[key].Name);
                    GetKPLInformation(device.Address);
                    log.InfoFormat("Returned from  Get KPL Information, Button Mask: " + ((IMultiButtonDevice)device).KPLButtonMask.ToString("X"));
                    Thread.Sleep(300);
                }
            }
        }

        public void GetThermostatExtendedInformation(DeviceAddress deviceAddress)
        {
            byte[] command = new byte[22];
            try
            {
                // send a get status command
                command[0] = 0x02;
                command[1] = 0x62;
                command[2] = deviceAddress.Byte1;
                command[3] = deviceAddress.Byte2;
                command[4] = deviceAddress.Byte3;
                command[5] = 0x1F;
                command[6] = Constants.EXT_COMMAND_EXTENDED_GET_SET;
                command[7] = 0x00;
                command[8] = 0x00;

                _plm.Write(command, 0, 22);

                _lastSentCommand = command;

                _gettingThermostatExtended = true;

                _extendedGetWaitHandle.Reset();
                log.Info("Calling WaitOne on ExtendedGetWaitHandle");
                _extendedGetWaitHandle.WaitOne(5000);
                log.Info("ExtendedGetWaitHandle finished");


                _gettingThermostatExtended = false;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error getting thermostat extended Information for device: {0}", deviceAddress.ToString()));
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
            }
        }


        public void GetKPLInformation(DeviceAddress deviceAddress)
        {
            byte[] command = new byte[22];
            try
            {
                // send a get status command
                command[0] = 0x02;
                command[1] = 0x62;
                command[2] = deviceAddress.Byte1;
                command[3] = deviceAddress.Byte2;
                command[4] = deviceAddress.Byte3;
                command[5] = 0x1F;
                command[6] = Constants.EXT_COMMAND_EXTENDED_GET_SET;
                command[7] = 0x00;
                command[8] = 0x01;

                _plm.Write(command, 0, 22);

                _lastSentCommand = command;

                _gettingExtendedStatus = true;

                _extendedGetWaitHandle.Reset();
                log.Info("Calling WaitOne on ExtendedGetWaitHandle");
                _extendedGetWaitHandle.WaitOne(5000);
                log.Info("ExtendedGetWaitHandle finished");


                _gettingExtendedStatus = false;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error getting KPL Information for device: {0}", deviceAddress.ToString()));
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
            }
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


        private void SerializeDeviceALDB()
        {
            TextWriter tw = null;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(_aldbLibrary.GetType());
                tw = new StreamWriter(_deviceALDBPath);
                xmlSerializer.Serialize(tw, _aldbLibrary);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error serializing Device ALDB: {0},\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (null != tw)
                {
                    tw.Close();
                    tw.Dispose();
                }
            }
        }

        private void DeserializeStoredDeviceALDB()
        {
            TextReader tr = null;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ALDBLibrary));

                if (!File.Exists(_deviceALDBPath))
                    return;

                tr = new StreamReader(_deviceALDBPath);
                _aldbLibrary = (ALDBLibrary)xmlSerializer.Deserialize(tr);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error deserializing Device ALDB: {0},\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (null != tr)
                {
                    tr.Close();
                    tr.Dispose();
                }
            }
        }

        private void DeviceTimerCallBack(Device device)
        {
            SlapsteonEventLog.AddLogEntry(new SlapsteonEventLogEntry(device.Name,
                string.Format("Turned off by timer.")));

            device.Status = 0;
            device.LastOff = DateTime.Now;
            this.SendStandardCommand(device.Address, Constants.STD_COMMAND_FAST_OFF, 0x00, 0x0F);
            this.ProcessSendingRelatedEvents(Constants.STD_COMMAND_FAST_OFF, device);
        }

        private void ProcessAllLinkResponse(byte[] allLinkMessage)
        {
            // 02 57 A2/E2 GRP ADR1 ADR2 ADR3 Local Data

            DeviceAddress address = new DeviceAddress(allLinkMessage[4], allLinkMessage[5], allLinkMessage[6]);
            LinkRecord linkRecord = new LinkRecord(allLinkMessage[2], allLinkMessage[3], address, allLinkMessage[7], allLinkMessage[8], allLinkMessage[9]);

            linkRecord.ReferenceDeviceName = GetDeviceName(address.StringAddress);

            _imLinks.Add(linkRecord);
        }
    }

   
}
