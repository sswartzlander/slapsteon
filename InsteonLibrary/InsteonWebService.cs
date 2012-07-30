using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Configuration;
using System.ServiceModel;
using log4net;

namespace Insteon.Library
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class InsteonWebService : IInsteonWebService
    {
        private static readonly ILog log = LogManager.GetLogger("Insteon");
        private readonly string _serialPort = "COM4";
        private static DeviceAddress _gameroomDimmer = new DeviceAddress(0x1B, 0xBC, 0xC0);
        private static DeviceAddress _livingroomDimmer = new DeviceAddress(0x1B, 0xBE, 0xCC);
        private static DeviceAddress _mbrDimmer = new DeviceAddress(0x1B, 0xB0, 0xB9);
        private static DeviceAddress _MBRMulti = new DeviceAddress(0x19, 0x2B, 0xD4);
        private static DeviceAddress _kitchenMultiSolo = new DeviceAddress(0x19, 0x2B, 0x89);
        private static DeviceAddress _kitchenMulti = new DeviceAddress(0x19, 0x2A, 0x4D);
        private static DeviceAddress _breakfastDimmer = new DeviceAddress(0x1B, 0xBF, 0x6E);
        private static DeviceAddress _plmAddress = new DeviceAddress(0x19, 0x77, 0x51);
        private static DeviceAddress _coachLights = new DeviceAddress(0x17, 0xF3, 0x23);
        private static DeviceAddress _frontDoorHigh = new DeviceAddress(0x19, 0x2B, 0x83);

        private static SerialPort _plm;

        private static object _serialLock = new object();

        public InsteonWebService(SerialPort plm)
        {
            _plm = plm;
        }

        public void GameroomDimmerOn()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _gameroomDimmer);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        public void GameroomDimmerOff()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOff(_plm, _gameroomDimmer);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void LivingRoomDimmerOn()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _livingroomDimmer);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                log.Error("Error occurred in LivingRoomDimmerOn: " + ex.Message);
                log.Error(ex.StackTrace);
                
            }
        }

        public void LivingRoomDimmerOff()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOff(_plm, _livingroomDimmer);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void LivingRoomDimmerRampOn()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                RampOn(_plm, _livingroomDimmer, 0xF0, 0x05);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void LivingRoomDimmerRampOff()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                RampOff(_plm, _livingroomDimmer, 0x05);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRDimmerOff()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOff(_plm, _mbrDimmer);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRDimmerOn100()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _mbrDimmer);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRDimmerOn30()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _mbrDimmer, 0x4c);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRDimmerRampOn()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                RampOn(_plm, _mbrDimmer, 0xF0, 0x03);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRDimmerRampOff()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                RampOff(_plm, _mbrDimmer, 0x04);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRDimmerOn40()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _mbrDimmer, 0x68);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRDimmerOn70()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _mbrDimmer, 0xB3);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void KitchenMultiOn()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _kitchenMulti);
                Thread.Sleep(200);
                FastOn(_plm, _kitchenMultiSolo);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void KitchenMultiOff()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOff(_plm, _kitchenMulti);
                Thread.Sleep(200);
                FastOff(_plm, _kitchenMultiSolo);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRMultiOn()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOn(_plm, _MBRMulti);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void MBRMultiOff()
        {
            try
            {
                //SerialPort plm = new SerialPort(_serialPort, 19200, Parity.None, 8, StopBits.One);
                //plm.Open();

                FastOff(_plm, _MBRMulti);

                //plm.Close();
                //plm.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public string GetAddressTable(string name)
        {
            string addressTable = "Bad device specified";

            DeviceAddress insteonAddress = null;

            switch (name.ToUpper())
            {
                case "GAMEROOMDIMMER":
                    insteonAddress = _gameroomDimmer;
                    break;
                case "LIVINGROOMDIMMER":
                    insteonAddress = _livingroomDimmer;
                    break;
                case "MBRDIMMER":
                    insteonAddress = _mbrDimmer;
                    break;
                case "MBRMULTI":
                    insteonAddress = _MBRMulti;
                    break;
                case "KITCHENMULTISOLO":
                    insteonAddress = _kitchenMultiSolo;
                    break;
                case "KITCHENMULTI":
                    insteonAddress = _kitchenMulti;
                    break;
                case "BREAKFASTDIMMER":
                    insteonAddress = _breakfastDimmer;
                    break;
                case "COACHLIGHTS":
                    insteonAddress = _coachLights;
                    break;
                case "FRONTDOORHIGH":
                    insteonAddress = _frontDoorHigh;
                    break;
                case "PLM":
                    insteonAddress = _plmAddress;
                    break;
                default:
                    return addressTable;
            }



            try
            {

                lock (_serialLock)
                {
                    byte[] cmdBytes = new byte[22];
                    cmdBytes[0] = 0x02;
                    cmdBytes[1] = 0x62;
                    cmdBytes[2] = insteonAddress.Byte1;
                    cmdBytes[3] = insteonAddress.Byte2;
                    cmdBytes[4] = insteonAddress.Byte3;
                    cmdBytes[5] = 0x13;
                    cmdBytes[6] = 0x2F;
                    cmdBytes[7] = 0x00;
                    cmdBytes[8] = 0x00;
                    cmdBytes[9] = 0x00;
                    cmdBytes[10] = 0x00;
                    cmdBytes[11] = 0x00;
                    cmdBytes[12] = 0x00;

                    _plm.Write(cmdBytes, 0, 22);

                    Thread.Sleep(800);
                    int numberOfBytesToRead = _plm.BytesToRead;                  

                    byte[] bytesRead = new byte[numberOfBytesToRead];

                    _plm.Read(bytesRead, 0, numberOfBytesToRead);

                    addressTable = BitConverter.ToString(bytesRead);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                addressTable = "Error occurred: " + ex.Message;
            }

            return addressTable;
        }

        public void FastOn(SerialPort plm, DeviceAddress address)
        {
            FastOn(plm, address, 0xFF);
        }

        public void FastOn(SerialPort plm, DeviceAddress address, byte level)
        {
            log.Info("FastOn called");

            byte[] cmdBytes = new byte[8];
            cmdBytes[0] = 0x02;
            cmdBytes[1] = 0x62;
            cmdBytes[2] = address.Byte1;
            cmdBytes[3] = address.Byte2;
            cmdBytes[4] = address.Byte3;
            cmdBytes[5] = 0x03;
            cmdBytes[6] = 0x11;
            cmdBytes[7] = level;

            plm.Write(cmdBytes, 0, 8);

            int numberOfBytesToRead = plm.BytesToRead;

            byte[] bytesRead = new byte[numberOfBytesToRead];

            plm.Read(bytesRead, 0, numberOfBytesToRead);

            string byteString = BitConverter.ToString(bytesRead);
        }

        public void FastOff(SerialPort plm, DeviceAddress address)
        {
            byte[] cmdBytes = new byte[8];
            cmdBytes[0] = 0x02;
            cmdBytes[1] = 0x62;
            cmdBytes[2] = address.Byte1;
            cmdBytes[3] = address.Byte2;
            cmdBytes[4] = address.Byte3;
            cmdBytes[5] = 0x03;
            cmdBytes[6] = 0x14;
            cmdBytes[7] = 0xFF;

            plm.Write(cmdBytes, 0, 8);

            int numberOfBytesToRead = plm.BytesToRead;

            byte[] bytesRead = new byte[numberOfBytesToRead];

            plm.Read(bytesRead, 0, numberOfBytesToRead);

            string byteString = BitConverter.ToString(bytesRead);
        }

        public void RampOn(SerialPort plm, DeviceAddress address, byte brightness, byte rampRate)
        {
            byte rampByte = (byte)(brightness | rampRate);
            
            byte[] cmdBytes = new byte[8];
            cmdBytes[0] = 0x02;
            cmdBytes[1] = 0x62;
            cmdBytes[2] = address.Byte1;
            cmdBytes[3] = address.Byte2;
            cmdBytes[4] = address.Byte3;
            cmdBytes[5] = 0x03;
            cmdBytes[6] = 0x2E; // ramp on
            cmdBytes[7] = rampByte;

            plm.Write(cmdBytes, 0, 8);

            int numberOfBytesToRead = plm.BytesToRead;

            byte[] bytesRead = new byte[numberOfBytesToRead];

            plm.Read(bytesRead, 0, numberOfBytesToRead);

            string byteString = BitConverter.ToString(bytesRead);
        }

        public void RampOff(SerialPort plm, DeviceAddress address, byte rampRate)
        {
            byte rampByte = (byte)(0x0F & rampRate);

            byte[] cmdBytes = new byte[8];
            cmdBytes[0] = 0x02;
            cmdBytes[1] = 0x62;
            cmdBytes[2] = address.Byte1;
            cmdBytes[3] = address.Byte2;
            cmdBytes[4] = address.Byte3;
            cmdBytes[5] = 0x03;
            cmdBytes[6] = 0x2F; // ramp off
            cmdBytes[7] = rampByte;

            plm.Write(cmdBytes, 0, 8);

            int numberOfBytesToRead = plm.BytesToRead;

            byte[] bytesRead = new byte[numberOfBytesToRead];

            plm.Read(bytesRead, 0, numberOfBytesToRead);

            string byteString = BitConverter.ToString(bytesRead);
        }
    }
}
