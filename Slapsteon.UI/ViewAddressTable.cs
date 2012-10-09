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
        private List<AddressRecord> _aldb;
        private List<ALDBFriendlyEntry> _aldbFriendly = new List<ALDBFriendlyEntry>();
        public ViewAddressTable(string deviceName, List<AddressRecord> aldbRecords)
        {
            _aldb = aldbRecords;
            _deviceName = deviceName;
            InitializeComponent();
        }

        private void ViewAddressTable_Load(object sender, EventArgs e)
        {
            gbDeviceAddressTable.Text = _deviceName;

            foreach (AddressRecord record in _aldb)
            {
                ALDBFriendlyEntry entry = new ALDBFriendlyEntry(
                    record.Type.ToString(),
                    (int)record.GroupNumber,
                    record.AddressOffset,
                    record.LocalData1,
                    record.LocalData2,
                    record.LocalData3,
                    record.AddressDeviceName);

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

        private class ALDBFriendlyEntry
        {
            public ALDBFriendlyEntry(string type, int group, string offset, byte ld1, byte ld2, byte ld3, string targetDeviceName)
            {
                Type = type;
                Group = group;
                Offset = offset.PadRight(4,'0');
                LocalData1 = "0x" + ld1.ToString("X").PadRight(2,'0');
                LocalData2 = "0x" + ld2.ToString("X").PadRight(2, '0');
                LocalData3 = "0x" + ld3.ToString("X").PadRight(2, '0');
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
