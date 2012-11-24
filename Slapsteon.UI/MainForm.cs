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
            

            InitializeComponent();
            _handler = new InsteonHandler(_serialPort);
            _handler.EnableMonitorMode();
            Thread.Sleep(500);

            _handler.GetALDBForAllDevices();
            _handler.GetStatusForAllDevices();

            foreach (string key in _handler.AllDevices.Keys)
            {
                _allDevices.Add(_handler.AllDevices[key]);
            }
            
            _handler.InsteonTrafficDetected += new InsteonHandler.InsteonTrafficHandler(InsteonTrafficDetected);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            dgvDevices.DataSource =  _allDevices;
            dgvDevices.Columns["Address"].Visible = false;
            dgvDevices.Columns["Delta"].Visible = false;
            dgvDevices.Columns["IsPLM"].Visible = false;
            dgvDevices.Columns["IsDimmable"].Visible = false;
            dgvDevices.Columns["ALDB"].Visible = false;
            dgvDevices.Columns["LastOnString"].Visible = false;
            dgvDevices.Columns["LastOffString"].Visible = false;
            dgvDevices.Columns["DeltaString"].Visible = false;
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

            _handler.SendExtendedCommand(_selectedDevice.Address, cmd1, cmd2, flags, ud1, ud2, ud3, ud4, ud5, ud6, ud7, ud8,
                ud9, ud10, ud11, ud12, ud13, ud14);

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

                lblOnLevel.Text = status.OnLevel.ToString();
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
                ViewAddressTable viewAddressTable = new ViewAddressTable(_selectedDevice.Name,
                    _handler.AllDevices[_selectedDevice.Address.ToString()].ALDB.Values.ToList());
                viewAddressTable.ShowDialog();

            }
            catch (Exception ex)
            {

            }
        }
    }
}
