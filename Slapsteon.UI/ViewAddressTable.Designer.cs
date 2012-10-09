namespace Slapsteon.UI
{
    partial class ViewAddressTable
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
            this.gbDeviceAddressTable = new System.Windows.Forms.GroupBox();
            this.dgvALDB = new System.Windows.Forms.DataGridView();
            this.gbDeviceAddressTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvALDB)).BeginInit();
            this.SuspendLayout();
            // 
            // gbDeviceAddressTable
            // 
            this.gbDeviceAddressTable.Controls.Add(this.dgvALDB);
            this.gbDeviceAddressTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDeviceAddressTable.Location = new System.Drawing.Point(0, 0);
            this.gbDeviceAddressTable.Name = "gbDeviceAddressTable";
            this.gbDeviceAddressTable.Size = new System.Drawing.Size(511, 285);
            this.gbDeviceAddressTable.TabIndex = 0;
            this.gbDeviceAddressTable.TabStop = false;
            // 
            // dgvALDB
            // 
            this.dgvALDB.AllowUserToAddRows = false;
            this.dgvALDB.AllowUserToDeleteRows = false;
            this.dgvALDB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvALDB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvALDB.Location = new System.Drawing.Point(3, 16);
            this.dgvALDB.Name = "dgvALDB";
            this.dgvALDB.ReadOnly = true;
            this.dgvALDB.Size = new System.Drawing.Size(505, 266);
            this.dgvALDB.TabIndex = 0;
            // 
            // ViewAddressTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 285);
            this.Controls.Add(this.gbDeviceAddressTable);
            this.Name = "ViewAddressTable";
            this.Text = "Address Table";
            this.Load += new System.EventHandler(this.ViewAddressTable_Load);
            this.gbDeviceAddressTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvALDB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDeviceAddressTable;
        private System.Windows.Forms.DataGridView dgvALDB;
    }
}