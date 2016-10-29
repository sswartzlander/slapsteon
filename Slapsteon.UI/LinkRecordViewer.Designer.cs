namespace Slapsteon.UI
{
    partial class LinkRecordViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbLinkRecords = new System.Windows.Forms.GroupBox();
            this.dgvLinkRecords = new System.Windows.Forms.DataGridView();
            this.gbLinkRecords.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLinkRecords)).BeginInit();
            this.SuspendLayout();
            // 
            // gbLinkRecords
            // 
            this.gbLinkRecords.Controls.Add(this.dgvLinkRecords);
            this.gbLinkRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLinkRecords.Location = new System.Drawing.Point(0, 0);
            this.gbLinkRecords.Name = "gbLinkRecords";
            this.gbLinkRecords.Size = new System.Drawing.Size(292, 273);
            this.gbLinkRecords.TabIndex = 0;
            this.gbLinkRecords.TabStop = false;
            this.gbLinkRecords.Text = "Link Records";
            // 
            // dgvLinkRecords
            // 
            this.dgvLinkRecords.AllowUserToAddRows = false;
            this.dgvLinkRecords.AllowUserToDeleteRows = false;
            this.dgvLinkRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLinkRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLinkRecords.Location = new System.Drawing.Point(3, 16);
            this.dgvLinkRecords.Name = "dgvLinkRecords";
            this.dgvLinkRecords.ReadOnly = true;
            this.dgvLinkRecords.Size = new System.Drawing.Size(286, 254);
            this.dgvLinkRecords.TabIndex = 0;
            // 
            // LinkRecordViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.gbLinkRecords);
            this.Name = "LinkRecordViewer";
            this.Text = "Links";
            this.Load += new System.EventHandler(this.LinkRecordViewer_Load);
            this.gbLinkRecords.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLinkRecords)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbLinkRecords;
        private System.Windows.Forms.DataGridView dgvLinkRecords;
    }
}