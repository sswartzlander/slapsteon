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
    public partial class LinkRecordViewer : Form
    {

        private List<LinkRecord> _linkRecords;
        public LinkRecordViewer(List<LinkRecord> linkRecords)
        {
            _linkRecords = linkRecords;
            InitializeComponent();
        }

        private void LinkRecordViewer_Load(object sender, EventArgs e)
        {
            dgvLinkRecords.DataSource = _linkRecords;
            dgvLinkRecords.Columns["LocalData"].Visible = false;
            dgvLinkRecords.Columns["Address"].Visible = false;
        }
    }
}
