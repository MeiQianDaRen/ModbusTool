namespace ModbusAddressTool
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox groupSerial;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ComboBox cboPort;
        private System.Windows.Forms.Button btnRefreshPort;

        private System.Windows.Forms.Label lblBaud;
        private System.Windows.Forms.ComboBox cboBaud;

        private System.Windows.Forms.Label lblParity;
        private System.Windows.Forms.ComboBox cboParity;

        private System.Windows.Forms.Label lblDataBits;
        private System.Windows.Forms.ComboBox cboDataBits;

        private System.Windows.Forms.Label lblStopBits;
        private System.Windows.Forms.ComboBox cboStopBits;

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;

        private System.Windows.Forms.GroupBox groupFormat;
        private System.Windows.Forms.RadioButton radioInteger;
        private System.Windows.Forms.RadioButton radioHex;
        private System.Windows.Forms.RadioButton radioBinary;

        private System.Windows.Forms.GroupBox groupAddress;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;

        private System.Windows.Forms.Label lblFunction;
        private System.Windows.Forms.ComboBox cboFunction;

        private System.Windows.Forms.Label lblRegister;
        private System.Windows.Forms.TextBox txtRegister;

        private System.Windows.Forms.Label lblCurrentAddress;
        private System.Windows.Forms.TextBox txtCurrentAddress;

        private System.Windows.Forms.Label lblNewAddress;
        private System.Windows.Forms.TextBox txtNewAddress;

        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnWrite;

        private System.Windows.Forms.GroupBox groupDevices;
        private System.Windows.Forms.DataGridView dgvDevices;

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnReadAll;
        private System.Windows.Forms.Button btnWriteAll;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnExportCsv;

        private System.Windows.Forms.GroupBox groupLog;
        private System.Windows.Forms.RichTextBox txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.groupSerial = new System.Windows.Forms.GroupBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.cboPort = new System.Windows.Forms.ComboBox();
            this.btnRefreshPort = new System.Windows.Forms.Button();

            this.lblBaud = new System.Windows.Forms.Label();
            this.cboBaud = new System.Windows.Forms.ComboBox();

            this.lblParity = new System.Windows.Forms.Label();
            this.cboParity = new System.Windows.Forms.ComboBox();

            this.lblDataBits = new System.Windows.Forms.Label();
            this.cboDataBits = new System.Windows.Forms.ComboBox();

            this.lblStopBits = new System.Windows.Forms.Label();
            this.cboStopBits = new System.Windows.Forms.ComboBox();

            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();

            this.groupFormat = new System.Windows.Forms.GroupBox();
            this.radioInteger = new System.Windows.Forms.RadioButton();
            this.radioHex = new System.Windows.Forms.RadioButton();
            this.radioBinary = new System.Windows.Forms.RadioButton();

            this.groupAddress = new System.Windows.Forms.GroupBox();

            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();

            this.lblFunction = new System.Windows.Forms.Label();
            this.cboFunction = new System.Windows.Forms.ComboBox();

            this.lblRegister = new System.Windows.Forms.Label();
            this.txtRegister = new System.Windows.Forms.TextBox();

            this.lblCurrentAddress = new System.Windows.Forms.Label();
            this.txtCurrentAddress = new System.Windows.Forms.TextBox();

            this.lblNewAddress = new System.Windows.Forms.Label();
            this.txtNewAddress = new System.Windows.Forms.TextBox();

            this.btnRead = new System.Windows.Forms.Button();
            this.btnWrite = new System.Windows.Forms.Button();

            this.groupDevices = new System.Windows.Forms.GroupBox();
            this.dgvDevices = new System.Windows.Forms.DataGridView();

            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnReadAll = new System.Windows.Forms.Button();
            this.btnWriteAll = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();

            this.groupLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.RichTextBox();

            this.groupSerial.SuspendLayout();
            this.groupFormat.SuspendLayout();
            this.groupAddress.SuspendLayout();
            this.groupDevices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDevices)).BeginInit();
            this.groupLog.SuspendLayout();
            this.SuspendLayout();

            // groupSerial
            this.groupSerial.Location = new System.Drawing.Point(12, 12);
            this.groupSerial.Name = "groupSerial";
            this.groupSerial.Size = new System.Drawing.Size(900, 78);
            this.groupSerial.TabIndex = 0;
            this.groupSerial.TabStop = false;
            this.groupSerial.Text = "RS485 / Modbus RTU";

            // lblPort
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(15, 30);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(41, 12);
            this.lblPort.Text = "串口：";

            // cboPort
            this.cboPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPort.FormattingEnabled = true;
            this.cboPort.Location = new System.Drawing.Point(58, 26);
            this.cboPort.Name = "cboPort";
            this.cboPort.Size = new System.Drawing.Size(90, 20);

            // btnRefreshPort
            this.btnRefreshPort.Location = new System.Drawing.Point(154, 25);
            this.btnRefreshPort.Name = "btnRefreshPort";
            this.btnRefreshPort.Size = new System.Drawing.Size(48, 22);
            this.btnRefreshPort.Text = "刷新";
            this.btnRefreshPort.UseVisualStyleBackColor = true;
            this.btnRefreshPort.Click += new System.EventHandler(this.btnRefreshPort_Click);

            // lblBaud
            this.lblBaud.AutoSize = true;
            this.lblBaud.Location = new System.Drawing.Point(220, 30);
            this.lblBaud.Name = "lblBaud";
            this.lblBaud.Size = new System.Drawing.Size(53, 12);
            this.lblBaud.Text = "波特率：";

            // cboBaud
            this.cboBaud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaud.FormattingEnabled = true;
            this.cboBaud.Location = new System.Drawing.Point(273, 26);
            this.cboBaud.Name = "cboBaud";
            this.cboBaud.Size = new System.Drawing.Size(85, 20);

            // lblParity
            this.lblParity.AutoSize = true;
            this.lblParity.Location = new System.Drawing.Point(375, 30);
            this.lblParity.Name = "lblParity";
            this.lblParity.Size = new System.Drawing.Size(53, 12);
            this.lblParity.Text = "校验位：";

            // cboParity
            this.cboParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParity.FormattingEnabled = true;
            this.cboParity.Location = new System.Drawing.Point(428, 26);
            this.cboParity.Name = "cboParity";
            this.cboParity.Size = new System.Drawing.Size(70, 20);

            // lblDataBits
            this.lblDataBits.AutoSize = true;
            this.lblDataBits.Location = new System.Drawing.Point(510, 30);
            this.lblDataBits.Name = "lblDataBits";
            this.lblDataBits.Size = new System.Drawing.Size(53, 12);
            this.lblDataBits.Text = "数据位：";

            // cboDataBits
            this.cboDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDataBits.FormattingEnabled = true;
            this.cboDataBits.Location = new System.Drawing.Point(563, 26);
            this.cboDataBits.Name = "cboDataBits";
            this.cboDataBits.Size = new System.Drawing.Size(65, 20);

            // lblStopBits
            this.lblStopBits.AutoSize = true;
            this.lblStopBits.Location = new System.Drawing.Point(645, 30);
            this.lblStopBits.Name = "lblStopBits";
            this.lblStopBits.Size = new System.Drawing.Size(53, 12);
            this.lblStopBits.Text = "停止位：";

            // cboStopBits
            this.cboStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStopBits.FormattingEnabled = true;
            this.cboStopBits.Location = new System.Drawing.Point(698, 26);
            this.cboStopBits.Name = "cboStopBits";
            this.cboStopBits.Size = new System.Drawing.Size(65, 20);

            // btnConnect
            this.btnConnect.Location = new System.Drawing.Point(775, 20);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(55, 28);
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

            // btnDisconnect
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(836, 20);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(55, 28);
            this.btnDisconnect.Text = "断开";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);

            // groupFormat
            this.groupFormat.Location = new System.Drawing.Point(12, 96);
            this.groupFormat.Name = "groupFormat";
            this.groupFormat.Size = new System.Drawing.Size(260, 52);
            this.groupFormat.TabIndex = 1;
            this.groupFormat.TabStop = false;
            this.groupFormat.Text = "数据显示格式";

            // radioInteger
            this.radioInteger.AutoSize = true;
            this.radioInteger.Checked = true;
            this.radioInteger.Location = new System.Drawing.Point(18, 22);
            this.radioInteger.Name = "radioInteger";
            this.radioInteger.Size = new System.Drawing.Size(47, 16);
            this.radioInteger.TabStop = true;
            this.radioInteger.Text = "整数";
            this.radioInteger.UseVisualStyleBackColor = true;
            this.radioInteger.CheckedChanged += new System.EventHandler(this.DisplayFormatChanged);

            // radioHex
            this.radioHex.AutoSize = true;
            this.radioHex.Location = new System.Drawing.Point(91, 22);
            this.radioHex.Name = "radioHex";
            this.radioHex.Size = new System.Drawing.Size(59, 16);
            this.radioHex.Text = "十六进制";
            this.radioHex.UseVisualStyleBackColor = true;
            this.radioHex.CheckedChanged += new System.EventHandler(this.DisplayFormatChanged);

            // radioBinary
            this.radioBinary.AutoSize = true;
            this.radioBinary.Location = new System.Drawing.Point(175, 22);
            this.radioBinary.Name = "radioBinary";
            this.radioBinary.Size = new System.Drawing.Size(59, 16);
            this.radioBinary.Text = "二进制";
            this.radioBinary.UseVisualStyleBackColor = true;
            this.radioBinary.CheckedChanged += new System.EventHandler(this.DisplayFormatChanged);

            // groupAddress
            this.groupAddress.Location = new System.Drawing.Point(286, 96);
            this.groupAddress.Name = "groupAddress";
            this.groupAddress.Size = new System.Drawing.Size(626, 125);
            this.groupAddress.TabIndex = 2;
            this.groupAddress.TabStop = false;
            this.groupAddress.Text = "设备地址修改";

            // lblName
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(15, 27);
            this.lblName.Text = "名称：";

            // txtName
            this.txtName.Location = new System.Drawing.Point(60, 23);
            this.txtName.Size = new System.Drawing.Size(130, 21);

            // lblFunction
            this.lblFunction.AutoSize = true;
            this.lblFunction.Location = new System.Drawing.Point(205, 27);
            this.lblFunction.Text = "功能码：";

            // cboFunction
            this.cboFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFunction.Location = new System.Drawing.Point(260, 23);
            this.cboFunction.Size = new System.Drawing.Size(70, 20);

            // lblRegister
            this.lblRegister.AutoSize = true;
            this.lblRegister.Location = new System.Drawing.Point(345, 27);
            this.lblRegister.Text = "寄存器地址：";

            // txtRegister
            this.txtRegister.Location = new System.Drawing.Point(420, 23);
            this.txtRegister.Size = new System.Drawing.Size(85, 21);
            this.txtRegister.Text = "00D3";

            // lblCurrentAddress
            this.lblCurrentAddress.AutoSize = true;
            this.lblCurrentAddress.Location = new System.Drawing.Point(15, 63);
            this.lblCurrentAddress.Text = "当前地址：";

            // txtCurrentAddress
            this.txtCurrentAddress.Location = new System.Drawing.Point(75, 59);
            this.txtCurrentAddress.Size = new System.Drawing.Size(80, 21);
            this.txtCurrentAddress.Text = "1";

            // lblNewAddress
            this.lblNewAddress.AutoSize = true;
            this.lblNewAddress.Location = new System.Drawing.Point(175, 63);
            this.lblNewAddress.Text = "修改地址：";

            // txtNewAddress
            this.txtNewAddress.Location = new System.Drawing.Point(235, 59);
            this.txtNewAddress.Size = new System.Drawing.Size(80, 21);
            this.txtNewAddress.Text = "2";

            // btnRead
            this.btnRead.Location = new System.Drawing.Point(350, 54);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(100, 30);
            this.btnRead.Text = "读取当前地址";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);

            // btnWrite
            this.btnWrite.Location = new System.Drawing.Point(465, 54);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(100, 30);
            this.btnWrite.Text = "修改地址";
            this.btnWrite.UseVisualStyleBackColor = true;
            this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);

            // groupDevices
            this.groupDevices.Anchor =
                ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left) |
                System.Windows.Forms.AnchorStyles.Right)));

            this.groupDevices.Location = new System.Drawing.Point(12, 230);
            this.groupDevices.Name = "groupDevices";
            this.groupDevices.Size = new System.Drawing.Size(900, 280);
            this.groupDevices.TabIndex = 3;
            this.groupDevices.TabStop = false;
            this.groupDevices.Text = "设备列表";

            // dgvDevices
            this.dgvDevices.AllowUserToAddRows = false;
            this.dgvDevices.AllowUserToDeleteRows = false;
            this.dgvDevices.AllowUserToResizeRows = false;
            this.dgvDevices.Anchor =
                ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left) |
                System.Windows.Forms.AnchorStyles.Right)));

            this.dgvDevices.AutoSizeColumnsMode =
                System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            this.dgvDevices.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvDevices.ColumnHeadersHeightSizeMode =
                System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            this.dgvDevices.Location = new System.Drawing.Point(10, 20);
            this.dgvDevices.MultiSelect = false;
            this.dgvDevices.Name = "dgvDevices";
            this.dgvDevices.ReadOnly = true;
            this.dgvDevices.RowHeadersVisible = false;
            this.dgvDevices.SelectionMode =
                System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDevices.Size = new System.Drawing.Size(880, 195);

            this.dgvDevices.CellDoubleClick +=
                new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDevices_CellDoubleClick);

            // btnAdd
            this.btnAdd.Location = new System.Drawing.Point(10, 225);
            this.btnAdd.Size = new System.Drawing.Size(65, 30);
            this.btnAdd.Text = "添加";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(80, 225);
            this.btnDelete.Size = new System.Drawing.Size(65, 30);
            this.btnDelete.Text = "删除";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // btnClear
            this.btnClear.Location = new System.Drawing.Point(150, 225);
            this.btnClear.Size = new System.Drawing.Size(65, 30);
            this.btnClear.Text = "清空";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            // btnReadAll
            this.btnReadAll.Location = new System.Drawing.Point(225, 225);
            this.btnReadAll.Size = new System.Drawing.Size(85, 30);
            this.btnReadAll.Text = "读取全部";
            this.btnReadAll.Click += new System.EventHandler(this.btnReadAll_Click);

            // btnWriteAll
            this.btnWriteAll.Location = new System.Drawing.Point(320, 225);
            this.btnWriteAll.Size = new System.Drawing.Size(85, 30);
            this.btnWriteAll.Text = "修改全部";
            this.btnWriteAll.Click += new System.EventHandler(this.btnWriteAll_Click);

            // btnImport
            this.btnImport.Location = new System.Drawing.Point(415, 225);
            this.btnImport.Size = new System.Drawing.Size(65, 30);
            this.btnImport.Text = "导入";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);

            // btnExport
            this.btnExport.Location = new System.Drawing.Point(485, 225);
            this.btnExport.Size = new System.Drawing.Size(65, 30);
            this.btnExport.Text = "导出";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);

            // btnExportCsv
            this.btnExportCsv.Location = new System.Drawing.Point(555, 225);
            this.btnExportCsv.Size = new System.Drawing.Size(100, 30);
            this.btnExportCsv.Text = "导出修改记录";
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // groupLog
            this.groupLog.Anchor =
                ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom) |
                System.Windows.Forms.AnchorStyles.Left)));

            this.groupLog.Location = new System.Drawing.Point(12, 520);
            this.groupLog.Name = "groupLog";
            this.groupLog.Size = new System.Drawing.Size(900, 205);
            this.groupLog.TabIndex = 4;
            this.groupLog.TabStop = false;
            this.groupLog.Text = "通讯日志";

            // txtLog
            this.txtLog.Anchor =
                ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Bottom) |
                System.Windows.Forms.AnchorStyles.Left) |
                System.Windows.Forms.AnchorStyles.Right)));

            this.txtLog.BackColor = System.Drawing.Color.White;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.Location = new System.Drawing.Point(10, 20);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(880, 175);

            // MainForm
            this.AutoScaleDimensions =
                new System.Drawing.SizeF(6F, 12F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.ClientSize =
                new System.Drawing.Size(924, 740);

            this.Controls.Add(this.groupLog);
            this.Controls.Add(this.groupDevices);
            this.Controls.Add(this.groupAddress);
            this.Controls.Add(this.groupFormat);
            this.Controls.Add(this.groupSerial);

            this.MinimumSize = new System.Drawing.Size(940, 780);
            this.Name = "MainForm";
            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Text = "Modbus RS485 地址修改工具";

            this.FormClosing +=
                new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);

            this.Load +=
                new System.EventHandler(this.MainForm_Load);

            this.groupSerial.ResumeLayout(false);
            this.groupSerial.PerformLayout();

            this.groupFormat.ResumeLayout(false);
            this.groupFormat.PerformLayout();

            this.groupAddress.ResumeLayout(false);
            this.groupAddress.PerformLayout();

            this.groupDevices.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.dgvDevices)).EndInit();

            this.groupLog.ResumeLayout(false);

            this.ResumeLayout(false);
        }
    }
}
