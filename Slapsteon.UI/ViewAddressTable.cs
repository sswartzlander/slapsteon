using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Insteon.Library;

namespace Slapsteon.UI
{
    public partial class ViewAddressTable : Form
    {
        private string _deviceName;
        private DeviceALDB _aldb;
        private List<ALDBFriendlyEntry> _aldbFriendly = new List<ALDBFriendlyEntry>();
        private Dictionary<string, Device> _allDevices;
        public ViewAddressTable(string deviceName, DeviceALDB deviceALDB, Dictionary<string,Device> allDevices)
        {
            _aldb = deviceALDB;
            _deviceName = deviceName;
            _allDevices = allDevices;
            InitializeComponent();
        }

        private void ViewAddressTable_Load(object sender, EventArgs e)
        {
            gbDeviceAddressTable.Text = _deviceName;

            foreach (ALDBRecord record in _aldb.ALDBRecords)
            {
                ALDBFriendlyEntry entry = new ALDBFriendlyEntry(
                    record.Flags.ToString("X"),
                    (int)record.Group,
                    record.AddressMSB.ToString("X") + record.AddressLSB.ToString("X"),
                    record.LocalData1,
                    record.LocalData2,
                    record.LocalData3,
                    GetTargetDeviceName(record.Address1, record.Address2, record.Address3));

                _aldbFriendly.Add(entry);
            }

            dgvALDB.DataSource = _aldbFriendly;
            dgvALDB.ColumnHeadersVisible = true;
            dgvALDB.Columns["LocalData1"].Width = 60;
            dgvALDB.Columns["LocalData1"].HeaderText = "";
            dgvALDB.Columns["LocalData2"].Width = 60;
            dgvALDB.Columns["LocalData2"].HeaderText = "";
            dgvALDB.Columns["LocalData3"].Width = 60;
            dgvALDB.Columns["LocalData3"].HeaderText = "";
            dgvALDB.Columns["Offset"].Width = 40;
            dgvALDB.Columns["Offset"].HeaderText = "Offset";

            dgvALDB.Columns["Group"].Width = 35;
            dgvALDB.Columns["Group"].HeaderText = "Grp";

            dgvALDB.Columns["TargetDeviceName"].HeaderText = "Target";
        }

        private string GetTargetDeviceName(byte address1, byte address2, byte address3)
        {
            string addressString = address1.ToString("X").PadLeft(2, '0') + address2.ToString("X").PadLeft(2, '0') + address3.ToString("X").PadLeft(2, '0');

            if (!_allDevices.ContainsKey(addressString)) return "unknown";

            return _allDevices[addressString].Name;
        }

        private class ALDBFriendlyEntry
        {
            public ALDBFriendlyEntry(string type, int group, string offset, byte ld1, byte ld2, byte ld3, string targetDeviceName)
            {
                Type = type;
                Group = group;
                Offset = offset.PadRight(4,'0');
                LocalData1 = "0x" + ld1.ToString("X").PadLeft(2,'0');
                LocalData2 = "0x" + ld2.ToString("X").PadLeft(2, '0');
                LocalData3 = "0x" + ld3.ToString("X").PadLeft(2, '0');
                TargetDeviceName = targetDeviceName;
            }

            public string Type { get; set; }
            public string Offset { get; set; }
            public string LocalData1 { get; set; }
            public string LocalData2 { get; set; }
            public string LocalData3 { get; set; }
            public int Group { get; set; }
            public string TargetDeviceName { get; set; }
        }
    }
}
