using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Insteon.Library;
using System.Threading;
using log4net;

namespace Slapsteon.UI
{
    public partial class MainForm : Form
    {
        private delegate void InsertConsoleTextCallback(string text); 
        private string _serialPort = "COM3";
        private object _serialReadLock = new object();
        private List<Device> _allDevices = new List<Device>();
        private readonly ILog log = LogManager.GetLogger("Insteon");
        private InsteonHandler _handler;
        private int _deviceSelectedIndex = -1;
        private Device _selectedDevice;
        private bool _readingALDB = false;

        public MainForm()
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Info("Starting application.");
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

            InitializeComponent();
            _handler = new InsteonHandler(_serialPort, _allDevices);
            _handler.EnableMonitorMode();
            Thread.Sleep(500);
            _handler.GetALDBForAllDevices();

            _handler.InsteonTrafficDetected += new InsteonHandler.InsteonTrafficHandler(InsteonTrafficDetected);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            dgvDevices.DataSource = _allDevices;
            dgvDevices.Columns["Address"].Visible = false;
            //dgcName.
        }

        private void btnSendStdCommand_Click(object sender, EventArgs e)
        {
            byte cmd1 = StringToByte(txtCmd1.Text);
            byte cmd2 = StringToByte(txtCmd2.Text);
            byte flags = StringToByte(txtFlags.Text);

            string commandResult = _handler.SendStandardCommand(_selectedDevice.Address, cmd1, cmd2, flags);

            rtCommandResults.Text = commandResult;
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

        private void button1_Click(object sender, EventArgs e)
        {
            byte cmd1 = StringToByte(txtCmd1.Text);
            byte cmd2 = StringToByte(txtCmd2.Text);
            byte flags = StringToByte(txtFlags.Text);
            byte ud1 = StringToByte(txtUD1.Text);
            byte ud2 = StringToByte(txtUD2.Text);
            byte ud3 = StringToByte(txtUD3.Text);
            byte ud4 = StringToByte(txtUD4.Text);
            byte ud5 = StringToByte(txtUD5.Text);
            byte ud6 = StringToByte(txtUD6.Text);
            byte ud7 = StringToByte(txtUD7.Text);
            byte ud8 = StringToByte(txtUD8.Text);
            byte ud9 = StringToByte(txtUD9.Text);
            byte ud10 = StringToByte(txtUD10.Text);
            byte ud11 = StringToByte(txtUD11.Text);
            byte ud12 = StringToByte(txtUD12.Text);
            byte ud13 = StringToByte(txtUD13.Text);
            byte ud14 = StringToByte(txtUD14.Text);

            string commandResult = _handler.SendExtendedCommand(_selectedDevice.Address, cmd1, cmd2, flags, ud1, ud2, ud3, ud4, ud5, ud6, ud7, ud8,
                ud9, ud10, ud11, ud12, ud13, ud14);

            rtCommandResults.Text = commandResult;

        }

        private void dgvDevices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // index didn't change
                if (e.RowIndex == _deviceSelectedIndex)
                    return;


                _deviceSelectedIndex = e.RowIndex;


                _selectedDevice = _allDevices.First(d => d.AddressString == dgvDevices.Rows[e.RowIndex].Cells["AddressString"].Value.ToString());

                lblName.Text = _selectedDevice.Name;
                lblAddress.Text = _selectedDevice.AddressString;
            }
            catch (Exception ex)
            {

            }
        }

        private void btnGetStatus_Click(object sender, EventArgs e)
        {
            try
            {
                DeviceStatus status = _handler.GetDeviceStatus(_selectedDevice.Address);

                lblOnLevel.Text = status.OnLevel.ToString("X");
                lblDelta.Text = status.Delta.ToString("X");
            }
            catch (Exception ex)
            {

            }
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            rtConsole.Text = "";
        }

        public void InsteonTrafficDetected(object sender, InsteonTrafficEventArgs e)
        {
            this.InsertConsoleText(e.ToString() + Environment.NewLine);
        }


        private void InsertConsoleText(string text)
        {
            if (this.rtConsole.InvokeRequired)
            {
                InsertConsoleTextCallback d = new InsertConsoleTextCallback(InsertConsoleText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.rtConsole.Text += text;
            }
        }

        private void btnGetAddressTable_Click(object sender, EventArgs e)
        {
            try
            {
                _handler.GetAddressRecords(_selectedDevice.Address);

                _handler.LastALDBEntryRead += new InsteonHandler.InsteonTrafficHandler(_handler_LastALDBEntryRead);

                DateTime addressReadStartTime = DateTime.Now;
                _readingALDB = true;
                while (true)
                {
                    // if more than 10 seconds have elapsed stop waiting on records
                    if (addressReadStartTime.AddSeconds(10) < DateTime.Now)
                    {
                        log.Warn("Giving up after waiting 10 seconds for ALDB read.");
                        break;
                    }

                    if (!_readingALDB)
                        break;
                }

                _handler.LastALDBEntryRead -= new InsteonHandler.InsteonTrafficHandler(_handler_LastALDBEntryRead);
            }
            catch (Exception ex)
            {

            }
        }

        void _handler_LastALDBEntryRead(object sender, InsteonTrafficEventArgs e)
        {
            _readingALDB = false;

        }
    }
}
