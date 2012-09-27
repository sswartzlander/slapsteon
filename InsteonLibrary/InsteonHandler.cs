﻿using System;
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

        public delegate void InsteonTrafficHandler(object sender, InsteonTrafficEventArgs e);

        public event InsteonTrafficHandler InsteonTrafficDetected;

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

                    _lastSentCommand = null;
                    return;
                }
                else if (commandBytes[_lastSentCommand.Length] == 0x15)
                {
                    log.Warn(string.Format("Received NAK for Command {0}", BytesToString(commandBytes)));
                    _lastSentCommand = null;
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

                byte[] singleCommand = new byte[25];
                for (int i = 0; i < 25; i++)
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

            if (null != InsteonTrafficDetected)
            {
                InsteonTrafficEventArgs args = new InsteonTrafficEventArgs();
                args.Source = _allDevices[fromAddress.ToString()];
                args.Destination = toAddress;
                args.Flags = flag;
                args.Command1 = command1;
                args.Command2 = command2;
                args.Description = string.Format("Command={0}/DestName={1}", commandType, GetDeviceName(toAddress.ToString()));
                InsteonTrafficDetected(this, args);
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

        public DeviceStatus GetDeviceStatus(DeviceAddress deviceAddress)
        {
            byte[] command = new byte[8];
            DeviceStatus status = new DeviceStatus();
            try
            {
                lock (this)
                {
                    // send a get status command
                    command[0] = 0x02;
                    command[1] = 0x62;
                    command[2] = deviceAddress.Byte1;
                    command[3] = deviceAddress.Byte2;
                    command[4] = deviceAddress.Byte3;
                    command[5] = 0x03;
                    command[6] = Constants.STD_COMMAND_STATUS_REQUEST;
                    command[7] = 0x00;

                    _plm.Write(command, 0, 8);

                    Thread.Sleep(1000);
                    int numberOfBytesToRead = _plm.BytesToRead;


                    if (numberOfBytesToRead < 20)
                        throw new Exception("Not enough data received.");

                    byte[] bytesRead = new byte[20]; // only need 20 bytes for the ack and response

                    _plm.Read(bytesRead, 0, 20);

                    string results = BitConverter.ToString(bytesRead);

                    // wait for the response

                    // 02-62-1B-BC-C0-03-19-00-06-02-50-1B-BC-C0-19-77-51-2B-03-00"

                    if (bytesRead[8] != 0x06)
                        throw new Exception("ACK bit was not 0x06 for status command.");

                    // read the command2 and 1 of the response

                    byte level = bytesRead[19];
                    byte delta = bytesRead[18];

                    status.OnLevel = level;
                    status.Delta = delta;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error getting device status for {0}", deviceAddress.ToString()));
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
            }

            return status;
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

            _plm.DataReceived += new SerialDataReceivedEventHandler(_plm_DataReceived);

            return;

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
        private byte[] _buffer = new byte[1024];
        private int _bufferOffset = 0;

        private void _plm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            log.Info("Data detected from serial port.");
            try
            {
                int bytesToRead = _plm.BytesToRead;
                byte[] bytesRead = new byte[bytesToRead];
                _plm.Read(bytesRead, 0, bytesToRead);

                string bytesAsString = BytesToString(bytesRead);
                log.Info(string.Format("{0} bytes read from buffer: {1}", bytesToRead, bytesAsString));

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
                    if (_buffer.Length < 2)
                        return;

                    if (_buffer[0] != 0x02)
                        throw new Exception("Buffer does not start with 0x02");

                    byte[] command = null;

                    switch (_buffer[1])
                    {
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
                            break;
                        case Constants.IM_COMMAND_EXTENDED_MESSAGE_RECEIVED:
                            // not enough data yet
                            if (_bufferOffset < 25)
                                return;
                            command = ExtractCommandFromBuffer(25);
                            break;
                        default:

                            break;
                    }

                    if (null != command)
                    {
                        string commandString = BytesToString(command);
                        log.Info("Received command " + commandString);
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

        public List<AddressRecord> GetAddressRecords(DeviceAddress address)
        {
            List<AddressRecord> addressRecords = new List<AddressRecord>();

            try
            {
                lock (this)
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

                    Thread.Sleep(1000);
                    int numberOfBytesToRead = _plm.BytesToRead;

                    // only read the ack (23 bytes) and we should receive a standard message (11)
                    if (numberOfBytesToRead > 34)
                        numberOfBytesToRead = 34; 

                    byte[] bytesRead = new byte[numberOfBytesToRead];

                    _plm.Read(bytesRead, 0, numberOfBytesToRead);

                    log.Info("Address Ack info: " + BytesToString(bytesRead));

                    while (true)
                    {
                        Thread.Sleep(250);
                        numberOfBytesToRead = _plm.BytesToRead;
                        if (numberOfBytesToRead >= 25)
                        {
                            byte[] addressEntryBytes = new byte[25];
                            _plm.Read(addressEntryBytes, 0, 25);

                            log.Info("Address data received: " + BytesToString(addressEntryBytes));

                            if (addressEntryBytes[16] != 0xA2 && addressEntryBytes[16] != 0xE2)
                            {
                                break;
                            }

                           

                            AddressEntryType type = (AddressEntryType)addressEntryBytes[16];

                            byte groupNumber = addressEntryBytes[17];
                            byte address1 = addressEntryBytes[18];
                            byte address2 = addressEntryBytes[19];
                            byte address3 = addressEntryBytes[20];

                            byte localData1 = addressEntryBytes[21];
                            byte localData2 = addressEntryBytes[22];
                            byte localData3 = addressEntryBytes[23];

                            AddressRecord record = new AddressRecord(type, groupNumber, 
                                new DeviceAddress(address1, address2, address3), 
                                localData1, localData2, localData3);

                            addressRecords.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return addressRecords;
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

    }

    
}