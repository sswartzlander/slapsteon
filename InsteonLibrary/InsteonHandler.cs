using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.IO.Ports;
using System.Threading;

namespace Insteon.Library
{
    public class InsteonHandler
    {

        private SerialPort _plm;
        private static readonly ILog log = LogManager.GetLogger("Insteon");
        private Thread _monitorModeThread;

        private Dictionary<string,Device> _allDevices = new Dictionary<string,Device>();

        public InsteonHandler(string comPort, List<Device> devices)
        {
            foreach (Device device in devices)
            {
                _allDevices.Add(device.Address.ToString(), device);
            }

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
        }

        private byte[] _lastSentCommand;

        public void ParseCommand(byte[] commandBytes)
        {
            //if (commandBytes.Length < 5)
            //{
            //    log.Error(string.Format("Received command not of sufficient length ({0}), command: {1}", commandBytes.Length, BytesToString(commandBytes)));
            //    return;
            //}

            if (commandBytes[0] != 0x02)
            {
                log.Error(string.Format("Received Command must start with 0x02.  Received command: {0}", BytesToString(commandBytes)));
                return;
            }

            // determine whether the command is an ack
            if (CompareToLastSentCommand(commandBytes))
            {
                // no need to parse ACK commands
                if (commandBytes[_lastSentCommand.Length] == 0x06)
                {
                    if (commandBytes.Length > _lastSentCommand.Length + 1)
                    {
                        byte[] remainingBytes = new byte[commandBytes.Length - (_lastSentCommand.Length + 1)];
                        Buffer.BlockCopy(commandBytes, _lastSentCommand.Length + 1, remainingBytes, 0, remainingBytes.Length);
                        ParseCommand(remainingBytes);

                    }
                    return;
                }
                else if (commandBytes[_lastSentCommand.Length] == 0x15)
                {
                    log.Warn(string.Format("Received NAK for Command {0}", BytesToString(commandBytes)));
                    return;
                }
            }

            if (0x50 == commandBytes[1]) // STD message received (11 bytes)
            {
                if (commandBytes.Length < 11)
                {
                    log.Error(string.Format("Bad standard command received {0}", BytesToString(commandBytes)));
                    return;
                }

                byte[] singleCommand = new byte[11];
                for (int i = 0; i < 11; i++)
                {
                    singleCommand[i]=commandBytes[i];

                }

                ProcessStandardReceivedMessage(singleCommand);

                if (commandBytes.Length > 11)
                {
                    byte[] remainingBytes = new byte[commandBytes.Length - 11];
                    Buffer.BlockCopy(commandBytes, 11, remainingBytes, 0, commandBytes.Length - 11);
                    ParseCommand(remainingBytes);
                }
            }
            else if (0x51 == commandBytes[1]) // EXT message received 
            {
                if (commandBytes.Length < 25)
                {
                    log.Error(string.Format("Bad extended command received {0}", BytesToString(commandBytes)));
                    return;
                }

                byte[] singleCommand = new byte[11];
                for (int i = 0; i < 11; i++)
                {
                    singleCommand[i]=commandBytes[i];
                }
            }
        }

        public void SetLastCommand(byte[] cmd)
        {
            _lastSentCommand = cmd;
        }

        private void ProcessStandardReceivedMessage(byte[] message)
        {
            DeviceAddress fromAddress = new DeviceAddress(message[2],message[3],message[4]);
            DeviceAddress toAddress = new DeviceAddress(message[5], message[6], message[7]);

            // check flags for group/device
            byte flagByte = (byte)(message[8] & (byte)0xE0);
            FlagsAck flag = (FlagsAck)flagByte;
            string flagDescription = "unknown";

            switch (flag)
            {
                case FlagsAck.DirectMessage:
                    flagDescription = "Direct Message";
                    break;
                case FlagsAck.ACKDirectMessage:
                    flagDescription = "ACK Direct Message";
                    break;
                case FlagsAck.ACKGroupCleanupDirectMessage:
                    flagDescription = "ACK Group Cleanup DM";
                    break;
                case FlagsAck.BroadcastMessage:
                    flagDescription = "Broadcast Message";
                    break;
                case FlagsAck.GroupBroadcastMessage:
                    flagDescription = "Group Broadcast Message";
                    break;
                case FlagsAck.GroupCleanupDirectMessage:
                    flagDescription = "Group Cleanup DM";
                    break;
                case FlagsAck.NAKDirectMessage:
                    flagDescription = "NAK Direct Message";
                    break;
                case FlagsAck.NAKGroupCleanupDirectMessage:
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
            Device dev = FindDeviceForAddress(toAddress.ToString());
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
                    commandType = "FastOff";
                    break;
                case Constants.STD_COMMAND_FAST_ON:
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
                    commandType = "LightRampOff";
                    break;
                case Constants.STD_COMMAND_LIGHT_RAMP_ON:
                    commandType = "LightRampOn";
                    break;

                case Constants.STD_COMMAND_LIGHT_SET_STATUS:
                    commandType = "LightSetStatus";
                    break;

                case Constants.STD_COMMAND_OFF:
                    if (null != dev)
                    {
                        dev.Status = 0;
                        dev.LastOff = DateTime.Now;
                    }
                    commandType = "CommandOff";
                    break;
                case Constants.STD_COMMAND_ON:
                    
                    if (null != dev)
                    {
                        dev.Status = 1;
                        dev.LastOn = DateTime.Now;
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

            log.Info(string.Format("Received Message from {0} to {1} of type: {2}({3}):{4} ({5})", GetDeviceName(fromAddress.ToString()), GetDeviceName(toAddress.ToString()), commandType, command1.ToString("X"), command2.ToString("X"), flagDescription));


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
            string results = null;
            byte[] command = new byte[8];

            Device targetDevice;
            if (!_allDevices.TryGetValue(deviceAddress.ToString(), out targetDevice))
                throw new Exception("No known device matched the specified address.");

            try
            {
                

                lock (this)
                {
                    command[0] = 0x02; // Insteon start byte
                    command[1] = 0x62; // Standard Command
                    command[2] = targetDevice.Address.Byte1;
                    command[3] = targetDevice.Address.Byte2;
                    command[4] = targetDevice.Address.Byte3;
                    command[5] = flags; // for standard 0x0F is good
                    command[6] = command1;
                    command[7] = command2;

                    _plm.Write(command, 0, 8);
                    Thread.Sleep(250);
                    log.Info(string.Format("Sent command {0} to device {1}.  (Command2: {2}, Flags: {3})", command1.ToString("X"), targetDevice.Name, command2.ToString("X"), flags.ToString("X")));
                    SetLastCommand(command);

                    int numberOfBytesToRead = _plm.BytesToRead;

                    byte[] bytesRead = new byte[numberOfBytesToRead];

                    _plm.Read(bytesRead, 0, numberOfBytesToRead);

                    results = BitConverter.ToString(bytesRead);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error sending command {0} to {1}. (Flags: {2})", command1.ToString("X"), targetDevice.Name, flags.ToString("X")), ex);
            }

            return results;
        }

        public string SendExtendedCommand(DeviceAddress deviceAddress, byte command1, byte command2, byte flags, byte ud1, byte ud2, byte ud3, byte ud4,
            byte ud5, byte ud6, byte ud7, byte ud8, byte ud9, byte ud10, byte ud11, byte ud12, byte ud13, byte ud14)
        {
            string results = null;
            byte[] command = new byte[22];

            Device targetDevice;
            if (!_allDevices.TryGetValue(deviceAddress.ToString(), out targetDevice))
                throw new Exception("No known device matched the specified address.");

            try
            {
                lock (this)
                {
                    command[0] = 0x02; // Insteon start byte
                    command[1] = 0x62; // Standard Command
                    command[2] = targetDevice.Address.Byte1;
                    command[3] = targetDevice.Address.Byte2;
                    command[4] = targetDevice.Address.Byte3;
                    command[5] = flags |= 0x10; // for standard 0x0F is good
                    command[6] = command1;
                    command[7] = command2;
                    command[8] = ud1;
                    command[9] = ud2;
                    command[10] = ud3;
                    command[11] = ud4;
                    command[12] = ud5;
                    command[13] = ud6;
                    command[14] = ud7;
                    command[15] = ud8;
                    command[16] = ud9;
                    command[17] = ud10;
                    command[18] = ud11;
                    command[19] = ud12;
                    command[20] = ud13;
                    command[21] = ud14;
                    _plm.Write(command, 0, 22);
                    Thread.Sleep(250);
                    log.Info(string.Format("Sent command {0} to device {1}.  (Command2: {2}, Flags: {3})", command1.ToString("X"), targetDevice.Name, command2.ToString("X"), flags.ToString("X")));
                    SetLastCommand(command);
                    Thread.Sleep(10000);
                    int numberOfBytesToRead = _plm.BytesToRead;

                    byte[] bytesRead = new byte[numberOfBytesToRead];

                    _plm.Read(bytesRead, 0, numberOfBytesToRead);

                    results = BitConverter.ToString(bytesRead);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error sending command {0} to {1}. (Flags: {2})", command1.ToString("X"), targetDevice.Name, flags.ToString("X")), ex);
            }

            return results;
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
                    Thread.Sleep(250);

                    int numberOfBytesToRead = _plm.BytesToRead;

                    byte[] bytesRead = new byte[numberOfBytesToRead];

                    _plm.Read(bytesRead, 0, numberOfBytesToRead);

                    string byteString = BitConverter.ToString(bytesRead);

                    log.Info(string.Format("SetMonitorMode received: {0}", byteString));
                }
                succeeded = true;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred setting monitor mode", ex);
            }

            return succeeded;
        }

        public void EnableMonitorMode()
        {
            _monitorModeThread = new Thread(delegate()
            {
                RunMonitorMode();
            });

            _monitorModeThread.Start();
        }

        private void RunMonitorMode()
        {

            this.SetMonitorMode();
            Thread.Sleep(250);

            //byte[] statusCommand = new byte[8]; // (, , 
            //statusCommand[0] = 0x02;
            //statusCommand[1] = 0x62;
            //statusCommand[2] = 0x19;
            //statusCommand[3] = 0x2A;
            //statusCommand[4] = 0x4D;
            //statusCommand[5] = 0x0F;
            //statusCommand[6] = 0x19;
            //statusCommand[7] = 0x00;

            //_plm.Write(statusCommand, 0, 8);

            //Thread.Sleep(250);

            //handler.SetLastCommand(statusCommand);
            int numberOfBytesToRead;
            byte[] bytesRead = null;
            string byteString = null;
            while (true)
            {
                // lock here so we can let specific operations prevent this from stealing output
                lock (this)
                {
                    numberOfBytesToRead = _plm.BytesToRead;

                    if (numberOfBytesToRead > 0)
                    {
                        bytesRead = new byte[numberOfBytesToRead];
                        _plm.Read(bytesRead, 0, numberOfBytesToRead);
                        byteString = BitConverter.ToString(bytesRead);
                        Console.WriteLine(DateTime.Now.ToString() + ": " + byteString);
                        log.Info(byteString);

                        this.ParseCommand(bytesRead);
                    }
                }
                Thread.Sleep(1000);
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

    }

    
}
