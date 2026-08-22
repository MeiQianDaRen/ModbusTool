using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ModbusAddressTool
{
    public class MainForm : Form
    {
        private const string DefaultProfileFileName =
            "ModbusAddressDefault.json";

        private SerialPort _serialPort;
        private TcpClient _tcpClient;
        private UdpClient _udpClient;
        private IPEndPoint _udpRemoteEndPoint;

        private readonly List<DeviceProfile> _devices =
            new List<DeviceProfile>();

        private DataGridView _grid;

        private TableLayoutPanel _rootLayout;

        private ComboBox _cmbPort;
        private ComboBox _cmbTransport;
        private ComboBox _cmbBaud;
        private ComboBox _cmbParity;
        private ComboBox _cmbDataBits;
        private ComboBox _cmbStopBits;
        private TextBox _txtHost;
        private TextBox _txtNetworkPort;
        private Control _rtuSettingsPanel;
        private Control _networkSettingsPanel;

        private ComboBox _cmbReadFunction;
        private ComboBox _cmbWriteFunction;

        private TextBox _txtName;
        private TextBox _txtRegister;
        private TextBox _txtCurrentAddress;
        private TextBox _txtNewAddress;

        private TextBox _txtCustomReadFrame;
        private TextBox _txtCustomWriteFrame;
        private DataGridView _customDataGrid;
        private TableLayoutPanel _deviceEditorLayout;
        private Control _customFrameDetails;

        private CheckBox _chkCustomWriteFrame;
        private CheckBox _chkAutoCrc;
        private CheckBox _chkVerify;

        private RadioButton _radioInteger;
        private RadioButton _radioHex;
        private RadioButton _radioBinary;

        private Button _btnConnect;
        private Button _btnDisconnect;
        private Button _btnRefresh;

        private Button _btnAdd;
        private Button _btnDelete;
        private Button _btnRead;
        private Button _btnWrite;
        private Button _btnImport;
        private Button _btnExport;
        private Button _btnSaveDefault;
        private Button _btnAddCustomData;
        private Button _btnDeleteCustomData;

        private RichTextBox _txtLog;

        private int _selectedIndex = -1;
        private bool _suppressDeviceSelection;
        private bool _loadingDevice;
        private DeviceProfile _validatedDevice;
        private string _validatedReadSignature;

        private DisplayFormat _displayFormat =
            DisplayFormat.Integer;

        private enum DisplayFormat
        {
            Integer,
            Hex,
            Binary
        }

        public MainForm()
        {
            InitializeForm();

            InitializeSerialPanel();

            InitializeDevicePanel();

            InitializeLogPanel();

            InitializeDevices();

            RefreshPorts();

            SetConnectedState(false);

            Log("==================================================");
            Log("Modbus RTU RS485 地址修改工具启动");
            Log("默认通讯：9600 / None / 8Bits / 1Bit");
            Log("读取功能码：03");
            Log("写入功能码：06");
            Log("自定义数据帧：关闭");
            Log("==================================================");
        }

        // ============================================================
        // 窗体
        // ============================================================

        private void InitializeForm()
        {
            Text = "串口修改工具V1.1(浮锐欧) By:周工";
            Width = 1440;
            Height = 900;
            MinimumSize = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Microsoft YaHei UI", 9F);
            BackColor = Color.FromArgb(244, 247, 250);
            ForeColor = Color.FromArgb(36, 45, 54);
            FormClosing += MainForm_FormClosing;

            _rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14),
                BackColor = BackColor
            };
            _rootLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));
            _rootLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 106F));
            _rootLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));
            _rootLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 184F));
            Controls.Add(_rootLayout);
        }

        private enum TransportMode
        {
            Rtu,
            Tcp,
            Udp
        }

        // ============================================================
        // 串口
        // ============================================================

        private void InitializeSerialPanel()
        {
            var group = CreateSection("通讯连接");
            group.Margin = new Padding(0, 0, 0, 10);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(10, 8, 10, 8)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 184F));

            _cmbTransport = CreateDropDown();
            _cmbTransport.Items.AddRange(new object[] { "RTU", "TCP", "UDP" });
            _cmbTransport.SelectedItem = "RTU";
            _cmbTransport.SelectedIndexChanged += TransportChanged;
            panel.Controls.Add(
                CreateInlineField("方式", _cmbTransport, 132, 38),
                0,
                0);

            var settingsHost = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            var rtuSettings = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0)
            };
            _cmbPort = CreateDropDown();
            _btnRefresh = CreateButton("刷新", delegate { RefreshPorts(); });
            _btnRefresh.Width = 58;
            _cmbBaud = CreateDropDown();
            _cmbBaud.Items.AddRange(new object[]
            {
                "1200", "2400", "4800", "9600", "19200",
                "38400", "57600", "115200"
            });
            _cmbBaud.SelectedItem = "9600";
            _cmbParity = CreateDropDown();
            _cmbParity.Items.AddRange(
                new object[] { "None", "Even", "Odd", "Mark", "Space" });
            _cmbParity.SelectedItem = "None";
            _cmbDataBits = CreateDropDown();
            _cmbDataBits.Items.AddRange(new object[] { "8", "7" });
            _cmbDataBits.SelectedItem = "8";
            _cmbStopBits = CreateDropDown();
            _cmbStopBits.Items.AddRange(new object[] { "1", "1.5", "2" });
            _cmbStopBits.SelectedItem = "1";

            rtuSettings.Controls.Add(
                CreateInlineField("串口", _cmbPort, 120, 38));
            rtuSettings.Controls.Add(_btnRefresh);
            rtuSettings.Controls.Add(
                CreateInlineField("波特率", _cmbBaud, 126, 50));
            rtuSettings.Controls.Add(
                CreateInlineField("校验位", _cmbParity, 126, 50));
            rtuSettings.Controls.Add(
                CreateInlineField("数据位", _cmbDataBits, 112, 58));
            rtuSettings.Controls.Add(
                CreateInlineField("停止位", _cmbStopBits, 112, 58));
            _rtuSettingsPanel = rtuSettings;

            var networkSettings = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Visible = false,
                Margin = new Padding(0)
            };
            _txtHost = new TextBox
            {
                Text = "127.0.0.1",
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(3, 7, 3, 5)
            };
            _txtNetworkPort = new TextBox
            {
                Text = "502",
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(3, 7, 3, 5)
            };
            networkSettings.Controls.Add(
                CreateInlineField("IP 地址", _txtHost, 340, 64));
            networkSettings.Controls.Add(
                CreateInlineField("端口", _txtNetworkPort, 180, 48));
            networkSettings.Controls.Add(new Label
            {
                Text = "通过网络透传 Modbus RTU 数据帧",
                AutoSize = true,
                ForeColor = Color.FromArgb(112, 123, 135),
                Margin = new Padding(12, 13, 0, 0)
            });
            _networkSettingsPanel = networkSettings;

            settingsHost.Controls.Add(networkSettings);
            settingsHost.Controls.Add(rtuSettings);
            panel.Controls.Add(settingsHost, 1, 0);

            _btnConnect = CreateButton("连接", Connect);
            SetAccentButton(_btnConnect, Color.FromArgb(32, 123, 229));
            _btnDisconnect = CreateButton("断开", Disconnect);
            var connectionButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0, 4, 0, 0)
            };
            _btnConnect.Width = 84;
            _btnDisconnect.Width = 84;
            connectionButtons.Controls.Add(_btnConnect);
            connectionButtons.Controls.Add(_btnDisconnect);
            panel.Controls.Add(connectionButtons, 2, 0);

            group.Controls.Add(panel);
            _rootLayout.Controls.Add(group, 0, 0);
        }

        private Control CreateInlineField(
            string label,
            Control editor,
            int width,
            int labelWidth)
        {
            var field = new TableLayoutPanel
            {
                Width = width,
                Height = 42,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            field.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, labelWidth));
            field.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));
            AddLabel(field, label, 0, 0);
            field.Controls.Add(editor, 1, 0);
            return field;
        }

        private void TransportChanged(object sender, EventArgs e)
        {
            bool isRtu = GetTransportMode() == TransportMode.Rtu;
            _rtuSettingsPanel.Visible = isRtu;
            _networkSettingsPanel.Visible = !isRtu;
            if (isRtu)
                _rtuSettingsPanel.BringToFront();
            else
                _networkSettingsPanel.BringToFront();
        }

        private TransportMode GetTransportMode()
        {
            if (_cmbTransport != null && _cmbTransport.Text == "TCP")
                return TransportMode.Tcp;
            if (_cmbTransport != null && _cmbTransport.Text == "UDP")
                return TransportMode.Udp;
            return TransportMode.Rtu;
        }

        private ComboBox CreateFunctionCombo(
            string defaultValue)
        {
            var box =
                new ComboBox();

            box.Dock =
                DockStyle.Fill;

            box.DropDownStyle =
                ComboBoxStyle.DropDown;

            box.Items.AddRange(
                new object[]
                {
                    "01",
                    "02",
                    "03",
                    "04",
                    "05",
                    "06",
                    "0F",
                    "10",
                    "41",
                    "42"
                });

            box.Text =
                defaultValue;

            return box;
        }

        private void AddLabel(
            TableLayoutPanel panel,
            string text,
            int column,
            int row)
        {
            panel.Controls.Add(
                new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    AutoEllipsis = true,
                    UseMnemonic = false,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                column,
                row);
        }

        private ComboBox CreateDropDown()
        {
            return new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(3, 7, 3, 5)
            };
        }

        private GroupBox CreateSection(string title)
        {
            return new GroupBox
            {
                Text = title,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 61, 72),
                Font = Font,
                Padding = new Padding(8)
            };
        }

        private void SetAccentButton(Button button, Color color)
        {
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = color;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.08F);
        }

        private RadioButton CreateDisplayRadio(string text, bool isChecked)
        {
            return new RadioButton
            {
                Text = text,
                Checked = isChecked,
                AutoSize = true,
                Margin = new Padding(7, 0, 0, 0)
            };
        }

        private void AddFormLabel(
            TableLayoutPanel panel,
            string text,
            int column,
            int row)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(72, 83, 95),
                Font = Font,
                Margin = new Padding(0)
            }, column, row);
        }

        private TextBox CreateFrameTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(4, 4, 4, 3)
            };
        }

        // ============================================================
        // 主设备区域
        // ============================================================

        private void InitializeDevicePanel()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Size = new Size(1200, 500),
                SplitterDistance = 690,
                SplitterWidth = 10,
                Panel1MinSize = 500,
                Panel2MinSize = 410,
                BackColor = BackColor,
                Margin = new Padding(0)
            };

            var listGroup = CreateSection("设备列表");
            listGroup.Dock = DockStyle.Fill;
            listGroup.Margin = new Padding(0, 0, 5, 0);

            var listLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8, 4, 8, 8)
            };
            listLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            listLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

            var fileButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = new Padding(0)
            };
            _btnAdd = CreateButton("＋ 添加", AddDevice);
            SetAccentButton(_btnAdd, Color.FromArgb(32, 123, 229));
            _btnDelete = CreateButton("删除", DeleteDevice);
            SetAccentButton(_btnDelete, Color.FromArgb(214, 69, 65));
            _btnImport = CreateButton("导入", ImportDevices);
            _btnExport = CreateButton("导出", ExportDevices);
            _btnSaveDefault = CreateButton("保存默认", SaveDefaultDevice);
            _btnAdd.Width = 76;
            _btnDelete.Width = 58;
            _btnImport.Width = 58;
            _btnExport.Width = 58;
            _btnSaveDefault.Width = 80;
            fileButtons.Controls.Add(_btnAdd);
            fileButtons.Controls.Add(_btnDelete);
            fileButtons.Controls.Add(_btnImport);
            fileButtons.Controls.Add(_btnExport);
            fileButtons.Controls.Add(_btnSaveDefault);
            toolbar.Controls.Add(fileButtons, 0, 0);

            var display = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0, 7, 0, 0)
            };
            _radioInteger = CreateDisplayRadio("整数", true);
            _radioHex = CreateDisplayRadio("十六进制", false);
            _radioBinary = CreateDisplayRadio("二进制", false);
            _radioInteger.CheckedChanged += DisplayFormatChanged;
            _radioHex.CheckedChanged += DisplayFormatChanged;
            _radioBinary.CheckedChanged += DisplayFormatChanged;
            display.Controls.Add(_radioBinary);
            display.Controls.Add(_radioHex);
            display.Controls.Add(_radioInteger);
            display.Controls.Add(new Label
            {
                Text = "显示：",
                AutoSize = true,
                Padding = new Padding(0, 3, 2, 0),
                ForeColor = Color.FromArgb(92, 103, 115)
            });
            toolbar.Controls.Add(display, 1, 0);
            listLayout.Controls.Add(toolbar, 0, 0);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(226, 232, 238),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(236, 241, 246);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(50, 61, 72);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                _grid.ColumnHeadersDefaultCellStyle.BackColor;
            _grid.DefaultCellStyle.BackColor = Color.White;
            _grid.DefaultCellStyle.ForeColor = ForeColor;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 235, 252);
            _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 74, 126);
            _grid.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _grid.RowTemplate.Height = 34;
            AddColumn("Name", "设备名称", 125);
            AddColumn("Current", "当前地址", 75);
            AddColumn("New", "目标地址", 75);
            AddColumn("Register", "寄存器", 82);
            AddColumn("Status", "最近状态", 210);
            _grid.SelectionChanged += Grid_SelectionChanged;
            listLayout.Controls.Add(_grid, 0, 1);
            listGroup.Controls.Add(listLayout);
            split.Panel1.Controls.Add(listGroup);

            var editorGroup = CreateSection("设备参数");
            editorGroup.Dock = DockStyle.Fill;
            editorGroup.Margin = new Padding(5, 0, 0, 0);

            var editorHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                AutoScrollMinSize = new Size(0, 460),
                Margin = new Padding(0)
            };

            _deviceEditorLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10, 5, 10, 10)
            };
            _deviceEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108F));
            _deviceEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            _deviceEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 280F));

            var basics = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Margin = new Padding(0, 0, 0, 8)
            };
            basics.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74F));
            basics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            basics.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
            basics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 3; i++)
                basics.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));

            AddFormLabel(basics, "名称", 0, 0);
            _txtName = CreateTextBox(0);
            _txtName.Dock = DockStyle.Fill;
            basics.Controls.Add(_txtName, 1, 0);

            AddFormLabel(basics, "寄存器", 2, 0);
            _txtRegister = CreateTextBox(0);
            _txtRegister.Dock = DockStyle.Fill;
            basics.Controls.Add(_txtRegister, 3, 0);

            AddFormLabel(basics, "当前地址", 0, 1);
            _txtCurrentAddress = CreateTextBox(0);
            _txtCurrentAddress.Dock = DockStyle.Fill;
            basics.Controls.Add(_txtCurrentAddress, 1, 1);

            AddFormLabel(basics, "目标地址", 2, 1);
            _txtNewAddress = CreateTextBox(0);
            _txtNewAddress.Dock = DockStyle.Fill;
            basics.Controls.Add(_txtNewAddress, 3, 1);

            AddFormLabel(basics, "读取功能码", 0, 2);
            _cmbReadFunction = CreateFunctionCombo("03");
            _cmbReadFunction.Dock = DockStyle.Fill;
            basics.Controls.Add(_cmbReadFunction, 1, 2);

            AddFormLabel(basics, "写入功能码", 2, 2);
            _cmbWriteFunction = CreateFunctionCombo("06");
            _cmbWriteFunction.Dock = DockStyle.Fill;
            basics.Controls.Add(_cmbWriteFunction, 3, 2);
            _txtRegister.TextChanged += ValidationEditorChanged;
            _txtCurrentAddress.TextChanged += ValidationEditorChanged;
            _cmbReadFunction.TextChanged += ValidationEditorChanged;
            _cmbWriteFunction.TextChanged += ValidationEditorChanged;
            _deviceEditorLayout.Controls.Add(basics, 0, 0);

            var frameGroup = CreateSection("自定义数据帧");
            frameGroup.Margin = new Padding(0, 0, 0, 8);
            var frameContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8, 2, 8, 5)
            };
            frameContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            frameContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _chkCustomWriteFrame = new CheckBox
            {
                Text = "启用自定义修改帧",
                AutoSize = true,
                Margin = new Padding(3, 5, 0, 0)
            };
            _chkCustomWriteFrame.CheckedChanged += CustomFrameVisibilityChanged;
            frameContainer.Controls.Add(_chkCustomWriteFrame, 0, 0);

            var framePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Visible = false,
                Margin = new Padding(0)
            };
            framePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            framePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            framePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            framePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            framePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            framePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            AddFormLabel(framePanel, "读取帧", 0, 0);
            _txtCustomReadFrame = CreateFrameTextBox();
            _txtCustomReadFrame.TextChanged += ValidationEditorChanged;
            framePanel.Controls.Add(_txtCustomReadFrame, 1, 0);
            AddFormLabel(framePanel, "修改帧", 0, 1);
            _txtCustomWriteFrame = CreateFrameTextBox();
            _txtCustomWriteFrame.TextChanged += ValidationEditorChanged;
            framePanel.Controls.Add(_txtCustomWriteFrame, 1, 1);

            var checks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = new Padding(0)
            };
            _chkAutoCrc = new CheckBox
            {
                Text = "自动追加 CRC16",
                Checked = true,
                AutoSize = true,
                Margin = new Padding(3, 8, 14, 0)
            };
            _chkVerify = new CheckBox
            {
                Text = "修改后验证",
                Checked = true,
                AutoSize = true,
                Margin = new Padding(3, 8, 0, 0)
            };
            _chkAutoCrc.CheckedChanged += ValidationEditorChanged;
            checks.Controls.Add(_chkAutoCrc);
            checks.Controls.Add(_chkVerify);
            framePanel.Controls.Add(checks, 0, 2);
            framePanel.SetColumnSpan(checks, 2);
            var hint = new Label
            {
                Text = "示例：01 41 00 D0 02；数据中已包含 CRC 时请关闭自动追加。",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(112, 123, 135),
                Padding = new Padding(3, 0, 0, 0)
            };
            framePanel.Controls.Add(hint, 0, 3);
            framePanel.SetColumnSpan(hint, 2);
            _customFrameDetails = framePanel;
            frameContainer.Controls.Add(framePanel, 0, 1);
            frameGroup.Controls.Add(frameContainer);
            _deviceEditorLayout.Controls.Add(frameGroup, 0, 1);

            var customDataGroup = CreateSection("自定义修改数据");
            customDataGroup.Margin = new Padding(0, 0, 0, 8);
            var customDataLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8, 3, 8, 8)
            };
            customDataLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            customDataLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var customDataToolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = new Padding(0)
            };
            _btnAddCustomData = CreateButton("添加项目", AddCustomDataItem);
            _btnDeleteCustomData = CreateButton("删除项目", DeleteCustomDataItem);
            _btnAddCustomData.Width = 84;
            _btnDeleteCustomData.Width = 84;
            customDataToolbar.Controls.Add(_btnAddCustomData);
            customDataToolbar.Controls.Add(_btnDeleteCustomData);
            customDataToolbar.Controls.Add(new Label
            {
                Text = "目标值为空或相同时跳过",
                AutoSize = true,
                ForeColor = Color.FromArgb(112, 123, 135),
                Margin = new Padding(10, 10, 0, 0)
            });
            customDataLayout.Controls.Add(customDataToolbar, 0, 0);

            _customDataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.FromArgb(226, 232, 238),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 30,
                RowTemplate = { Height = 28 }
            };
            _customDataGrid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(236, 241, 246);
            _customDataGrid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(50, 61, 72);
            _customDataGrid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(218, 235, 252);
            _customDataGrid.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(20, 74, 126);
            AddCustomDataColumn("ItemName", "项目名", 130);
            AddCustomDataColumn("ItemRegister", "寄存器地址", 90);
            AddCustomDataColumn("ItemCurrent", "当前值", 80);
            AddCustomDataColumn("ItemTarget", "目标值", 80);
            _customDataGrid.CellValueChanged += CustomDataGrid_CellValueChanged;
            customDataLayout.Controls.Add(_customDataGrid, 0, 1);
            customDataGroup.Controls.Add(customDataLayout);
            _deviceEditorLayout.Controls.Add(customDataGroup, 0, 2);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0)
            };
            _btnRead = CreateButton("读取当前设备", ReadSelectedDevice);
            _btnWrite = CreateButton("修改当前设备", WriteSelectedDevice);
            SetAccentButton(_btnWrite, Color.FromArgb(32, 123, 229));
            actions.Controls.Add(_btnRead);
            actions.Controls.Add(_btnWrite);

            editorHost.Controls.Add(_deviceEditorLayout);
            var editorContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            editorContainer.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));
            editorContainer.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 58F));
            editorContainer.Controls.Add(editorHost, 0, 0);
            editorContainer.Controls.Add(actions, 0, 1);
            editorGroup.Controls.Add(editorContainer);
            split.Panel2.Controls.Add(editorGroup);
            _rootLayout.Controls.Add(split, 0, 1);
        }

        private void AddColumn(
            string name,
            string header,
            int width)
        {
            var column =
                new DataGridViewTextBoxColumn();

            column.Name =
                name;

            column.HeaderText =
                header;

            column.MinimumWidth =
                Math.Min(width, 70);

            column.FillWeight =
                width;

            column.ReadOnly =
                true;

            _grid.Columns.Add(
                column);
        }

        private void AddCustomDataColumn(
            string name,
            string header,
            int weight)
        {
            _customDataGrid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = name,
                    HeaderText = header,
                    FillWeight = weight,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
        }

        private void CustomFrameVisibilityChanged(
            object sender,
            EventArgs e)
        {
            if (_customFrameDetails == null ||
                _deviceEditorLayout == null)
                return;

            bool visible = _chkCustomWriteFrame.Checked;
            _customFrameDetails.Visible = visible;
            _deviceEditorLayout.RowStyles[1].Height = visible ? 204F : 72F;
            _deviceEditorLayout.PerformLayout();

            if (!_loadingDevice)
                InvalidateReadValidation();
        }

        private void ValidationEditorChanged(
            object sender,
            EventArgs e)
        {
            if (!_loadingDevice)
                InvalidateReadValidation();
        }

        private void CustomDataGrid_CellValueChanged(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (_loadingDevice || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = _customDataGrid.Columns[e.ColumnIndex].Name;
            if (columnName == "ItemRegister" || columnName == "ItemCurrent")
                InvalidateReadValidation();
        }

        private void AddCustomDataItem(
            object sender,
            EventArgs e)
        {
            int row = _customDataGrid.Rows.Add();
            _customDataGrid.Rows[row].Cells["ItemName"].Value =
                "自定义项" + (row + 1);
            _customDataGrid.Rows[row].Cells["ItemRegister"].Value = "0000";
            _customDataGrid.ClearSelection();
            _customDataGrid.Rows[row].Selected = true;
            _customDataGrid.CurrentCell =
                _customDataGrid.Rows[row].Cells["ItemName"];
            _customDataGrid.BeginEdit(true);
            InvalidateReadValidation();
        }

        private void DeleteCustomDataItem(
            object sender,
            EventArgs e)
        {
            if (_customDataGrid.SelectedRows.Count == 0)
                return;

            _customDataGrid.Rows.RemoveAt(
                _customDataGrid.SelectedRows[0].Index);
            InvalidateReadValidation();
        }

        private void MarkReadValidated(DeviceProfile device)
        {
            _validatedDevice = device;
            _validatedReadSignature = BuildReadValidationSignature(device);
            UpdateWriteButtonState();
        }

        private void InvalidateReadValidation()
        {
            _validatedDevice = null;
            _validatedReadSignature = null;
            UpdateWriteButtonState();
        }

        private bool IsReadValidated(DeviceProfile device)
        {
            return device != null &&
                object.ReferenceEquals(_validatedDevice, device) &&
                string.Equals(
                    _validatedReadSignature,
                    BuildReadValidationSignature(device),
                    StringComparison.Ordinal);
        }

        private string BuildReadValidationSignature(DeviceProfile device)
        {
            var text = new StringBuilder();
            text.Append(device.CurrentAddress).Append('|')
                .Append(device.RegisterAddress).Append('|')
                .Append(device.ReadFunctionCode).Append('|')
                .Append(device.WriteFunctionCode).Append('|')
                .Append(device.UseCustomWriteFrame).Append('|')
                .Append(device.CustomReadFrame).Append('|')
                .Append(device.CustomWriteFrame).Append('|')
                .Append(device.AutoAppendCrc);

            if (device.CustomRegisterItems != null)
            {
                foreach (CustomRegisterItem item in device.CustomRegisterItems)
                {
                    text.Append('|').Append(item.RegisterAddress).Append(':')
                        .Append(item.CurrentValue.HasValue
                            ? item.CurrentValue.Value.ToString(
                                CultureInfo.InvariantCulture)
                            : "-");
                }
            }
            return text.ToString();
        }

        private bool IsCommunicationConnected()
        {
            return (_serialPort != null && _serialPort.IsOpen) ||
                (_tcpClient != null && _tcpClient.Connected) ||
                _udpClient != null;
        }

        private void UpdateWriteButtonState()
        {
            if (_btnWrite == null)
                return;

            bool enabled = IsCommunicationConnected() &&
                _selectedIndex >= 0 &&
                _selectedIndex < _devices.Count &&
                IsReadValidated(_devices[_selectedIndex]);
            _btnWrite.Enabled = enabled;
            if (enabled)
            {
                SetAccentButton(_btnWrite, Color.FromArgb(32, 123, 229));
            }
            else
            {
                _btnWrite.BackColor = Color.FromArgb(218, 223, 229);
                _btnWrite.ForeColor = Color.FromArgb(125, 134, 144);
                _btnWrite.FlatAppearance.BorderColor =
                    Color.FromArgb(201, 208, 215);
            }
        }

        private void AddEditorLabel(
            FlowLayoutPanel panel,
            string text)
        {
            panel.Controls.Add(
                new Label
                {
                    Text = text,
                    Width = 60,
                    Height = 25,
                    Padding =
                        new Padding(0, 5, 0, 0)
                });
        }

        private TextBox CreateTextBox(
            int width)
        {
            return new TextBox
            {
                Width = width,
                Height = 27,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(4, 4, 4, 3)
            };
        }

        private Button CreateButton(
            string text,
            EventHandler handler)
        {
            var button =
                new Button();

            button.Text =
                text;

            button.Width =
                94;

            button.Height =
                32;

            button.FlatStyle =
                FlatStyle.Flat;

            button.BackColor =
                Color.FromArgb(239, 243, 247);

            button.ForeColor =
                Color.FromArgb(50, 61, 72);

            button.FlatAppearance.BorderColor =
                Color.FromArgb(207, 216, 225);

            button.Margin =
                new Padding(3, 3, 3, 3);

            button.Cursor =
                Cursors.Hand;

            button.Font =
                Font;

            button.Click +=
                handler;

            return button;
        }

        // ============================================================
        // 日志窗口
        // ============================================================

        private void InitializeLogPanel()
        {
            var group = CreateSection("通讯日志 / 调试信息");
            group.Margin = new Padding(0, 10, 0, 0);

            _txtLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                WordWrap = false,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(26, 34, 43),
                ForeColor = Color.FromArgb(206, 221, 234),
                BorderStyle = BorderStyle.None
            };

            group.Controls.Add(_txtLog);
            _rootLayout.Controls.Add(group, 0, 2);
        }

        // ============================================================
        // 初始化设备
        // ============================================================

        private void InitializeDevices()
        {
            if (!LoadDefaultDevices())
            {
                _devices.Add(
                    new DeviceProfile
                    {
                        Name = "设备1",
                        ReadFunctionCode = 3,
                        WriteFunctionCode = 6,
                        RegisterAddress = 0x00D0,
                        CurrentAddress = 1,
                        NewAddress = 2,
                        CustomReadFrame = "",
                        CustomWriteFrame = "",
                        UseCustomWriteFrame = false,
                        AutoAppendCrc = true,
                        VerifyAfterWrite = true
                    });
            }

            RefreshGrid();
            SelectDevice(0);
        }

        private bool LoadDefaultDevices()
        {
            string path = GetDefaultProfilePath();
            if (!File.Exists(path))
                return false;

            try
            {
                List<DeviceProfile> devices =
                    DeserializeDefaultDevices(path);
                if (devices == null || devices.Count == 0)
                    throw new InvalidDataException("默认配置文件格式错误。");

                _devices.AddRange(devices);
                Log(string.Format(
                    "已加载默认设备配置：{0}（{1} 台设备）",
                    path,
                    devices.Count));
                return true;
            }
            catch (Exception ex)
            {
                Log("默认设备配置加载失败，将使用内置配置：" + ex.Message);
                return false;
            }
        }

        private static List<DeviceProfile> DeserializeDefaultDevices(
            string path)
        {
            byte[] data = File.ReadAllBytes(path);
            try
            {
                var listSerializer = new DataContractJsonSerializer(
                    typeof(List<DeviceProfile>));
                using (var stream = new MemoryStream(data))
                {
                    return listSerializer.ReadObject(stream)
                        as List<DeviceProfile>;
                }
            }
            catch
            {
                var deviceSerializer = new DataContractJsonSerializer(
                    typeof(DeviceProfile));
                using (var stream = new MemoryStream(data))
                {
                    DeviceProfile device = deviceSerializer.ReadObject(stream)
                        as DeviceProfile;
                    return device == null
                        ? null
                        : new List<DeviceProfile> { device };
                }
            }
        }

        private static string GetDefaultProfilePath()
        {
            return Path.Combine(
                Path.GetDirectoryName(
                    typeof(MainForm).Assembly.Location),
                DefaultProfileFileName);
        }

        // ============================================================
        // 串口
        // ============================================================

        private void RefreshPorts()
        {
            string old =
                _cmbPort.Text;

            _cmbPort.Items.Clear();

            string[] ports =
                SerialPort.GetPortNames();

            Array.Sort(
                ports);

            foreach (string port in ports)
            {
                _cmbPort.Items.Add(
                    port);
            }

            if (!string.IsNullOrEmpty(old) &&
                _cmbPort.Items.Contains(old))
            {
                _cmbPort.SelectedItem =
                    old;
            }
            else if (_cmbPort.Items.Count > 0)
            {
                _cmbPort.SelectedIndex =
                    0;
            }

            Log(
                "串口扫描完成：" +
                (ports.Length == 0
                    ? "未发现串口"
                    : string.Join(
                        ", ",
                        ports)));
        }

        private void Connect(
            object sender,
            EventArgs e)
        {
            try
            {
                TransportMode mode = GetTransportMode();

                if (mode == TransportMode.Rtu)
                {
                    if (string.IsNullOrWhiteSpace(_cmbPort.Text))
                    {
                        MessageBox.Show("请选择串口。");
                        return;
                    }

                    Disconnect();

                    int baud = int.Parse(_cmbBaud.Text);
                    Parity parity = ParseParity(_cmbParity.Text);
                    int dataBits = int.Parse(_cmbDataBits.Text);
                    StopBits stopBits = ParseStopBits(_cmbStopBits.Text);

                    _serialPort = new SerialPort(
                        _cmbPort.Text,
                        baud,
                        parity,
                        dataBits,
                        stopBits);
                    _serialPort.ReadTimeout = 1500;
                    _serialPort.WriteTimeout = 1500;
                    _serialPort.Open();

                    SetConnectedState(true);
                    Log("RTU 串口连接成功：");
                    Log(string.Format(
                        "PORT={0}, Baud={1}, Parity={2}, DataBits={3}, StopBits={4}",
                        _cmbPort.Text,
                        baud,
                        parity,
                        dataBits,
                        stopBits));
                    return;
                }

                IPAddress address;
                int port;
                if (!IPAddress.TryParse(_txtHost.Text.Trim(), out address))
                {
                    MessageBox.Show("请输入有效的 IP 地址。");
                    return;
                }
                if (!int.TryParse(_txtNetworkPort.Text.Trim(), out port) ||
                    port < 1 || port > 65535)
                {
                    MessageBox.Show("端口必须为 1～65535。");
                    return;
                }

                Disconnect();
                if (mode == TransportMode.Tcp)
                {
                    _tcpClient = new TcpClient(address.AddressFamily);
                    _tcpClient.ReceiveTimeout = 1500;
                    _tcpClient.SendTimeout = 1500;
                    IAsyncResult result = _tcpClient.BeginConnect(
                        address,
                        port,
                        null,
                        null);
                    bool connected = result.AsyncWaitHandle.WaitOne(1500);
                    result.AsyncWaitHandle.Close();
                    if (!connected)
                    {
                        _tcpClient.Close();
                        _tcpClient = null;
                        throw new TimeoutException("TCP 连接超时。");
                    }
                    _tcpClient.EndConnect(result);
                }
                else
                {
                    _udpRemoteEndPoint = new IPEndPoint(address, port);
                    _udpClient = new UdpClient(address.AddressFamily);
                    _udpClient.Client.ReceiveTimeout = 1500;
                    _udpClient.Client.SendTimeout = 1500;
                    _udpClient.Connect(_udpRemoteEndPoint);
                }

                SetConnectedState(true);
                Log(string.Format(
                    "{0} 连接已就绪：{1}:{2}",
                    mode == TransportMode.Tcp ? "TCP" : "UDP",
                    address,
                    port));
            }
            catch (Exception ex)
            {
                Disconnect();
                Log("连接失败：" + ex.Message);
                MessageBox.Show("连接失败：\r\n" + ex.Message);
            }
        }

        private void Disconnect(
            object sender = null,
            EventArgs e = null)
        {
            bool hadConnection =
                (_serialPort != null && _serialPort.IsOpen) ||
                (_tcpClient != null && _tcpClient.Connected) ||
                _udpClient != null;

            try
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }

                    _serialPort.Dispose();
                }

                if (_tcpClient != null)
                    _tcpClient.Close();

                if (_udpClient != null)
                    _udpClient.Close();
            }
            catch
            {
            }

            _serialPort =
                null;

            _tcpClient = null;
            _udpClient = null;
            _udpRemoteEndPoint = null;

            SetConnectedState(
                false);

            if (hadConnection)
                Log("通讯连接已断开。");
        }

        private void SetConnectedState(
            bool connected)
        {
            if (_btnConnect == null)
                return;

            _btnConnect.Enabled =
                !connected;

            _btnDisconnect.Enabled =
                connected;

            _btnRefresh.Enabled =
                !connected;

            _cmbTransport.Enabled =
                !connected;

            _cmbPort.Enabled =
                !connected;

            _cmbBaud.Enabled =
                !connected;

            _cmbParity.Enabled =
                !connected;

            _cmbDataBits.Enabled =
                !connected;

            _cmbStopBits.Enabled =
                !connected;

            _txtHost.Enabled =
                !connected;

            _txtNetworkPort.Enabled =
                !connected;

            _btnRead.Enabled =
                connected;

            InvalidateReadValidation();
        }

        private static Parity ParseParity(
            string text)
        {
            switch (text)
            {
                case "Even":
                    return Parity.Even;

                case "Odd":
                    return Parity.Odd;

                case "Mark":
                    return Parity.Mark;

                case "Space":
                    return Parity.Space;

                default:
                    return Parity.None;
            }
        }

        private static StopBits ParseStopBits(
            string text)
        {
            switch (text)
            {
                case "1.5":
                    return StopBits.OnePointFive;

                case "2":
                    return StopBits.Two;

                default:
                    return StopBits.One;
            }
        }

        // ============================================================
        // CRC
        // ============================================================

        private static ushort CalculateCrc16(
            byte[] data)
        {
            ushort crc =
                0xFFFF;

            for (int i = 0;
                 i < data.Length;
                 i++)
            {
                crc ^= data[i];

                for (int j = 0;
                     j < 8;
                     j++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc >>= 1;

                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        private static byte[] AppendCrc(
            byte[] data)
        {
            ushort crc =
                CalculateCrc16(data);

            byte[] result =
                new byte[
                    data.Length + 2];

            Buffer.BlockCopy(
                data,
                0,
                result,
                0,
                data.Length);

            result[result.Length - 2] =
                (byte)(crc & 0xFF);

            result[result.Length - 1] =
                (byte)(crc >> 8);

            return result;
        }

        private static bool CheckCrc(
            byte[] frame)
        {
            if (frame == null ||
                frame.Length < 4)
                return false;

            byte[] data =
                new byte[
                    frame.Length - 2];

            Buffer.BlockCopy(
                frame,
                0,
                data,
                0,
                data.Length);

            ushort calculated =
                CalculateCrc16(data);

            ushort received =
                (ushort)(
                    frame[frame.Length - 2] |
                    (frame[frame.Length - 1] << 8));

            return calculated ==
                   received;
        }

        // ============================================================
        // 数据帧解析
        // ============================================================

        private static byte[] ParseFrame(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new byte[0];

            text =
                text.Replace(
                    ",",
                    " ");

            text =
                text.Replace(
                    "-",
                    " ");

            text =
                text.Replace(
                    "\r",
                    " ");

            text =
                text.Replace(
                    "\n",
                    " ");

            string[] parts =
                text.Split(
                    new char[]
                    {
                        ' ',
                        '\t'
                    },
                    StringSplitOptions.RemoveEmptyEntries);

            List<byte> bytes =
                new List<byte>();

            foreach (string item in parts)
            {
                string value =
                    item.Trim();

                if (value.StartsWith(
                    "0x",
                    StringComparison.OrdinalIgnoreCase))
                {
                    value =
                        value.Substring(2);
                }

                byte b;

                if (!byte.TryParse(
                    value,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out b))
                {
                    throw new FormatException(
                        "数据帧中存在无效字节：" +
                        item);
                }

                bytes.Add(b);
            }

            return bytes.ToArray();
        }

        private static byte[] BuildCustomFrame(
            string text,
            bool autoCrc)
        {
            byte[] data =
                ParseFrame(text);

            if (data.Length == 0)
            {
                throw new ArgumentException(
                    "数据帧不能为空。");
            }

            if (autoCrc)
            {
                return AppendCrc(
                    data);
            }

            return data;
        }

        // ============================================================
        // 标准 Modbus RTU
        // ============================================================

        private static byte[] BuildReadFrame(
            byte address,
            byte function,
            ushort register,
            ushort count)
        {
            byte[] frame =
            {
                address,
                function,

                (byte)(register >> 8),
                (byte)(register & 0xFF),

                (byte)(count >> 8),
                (byte)(count & 0xFF)
            };

            return AppendCrc(
                frame);
        }

        private static byte[] BuildWrite06Frame(
            byte address,
            byte function,
            ushort register,
            ushort value)
        {
            byte[] frame =
            {
                address,
                function,

                (byte)(register >> 8),
                (byte)(register & 0xFF),

                (byte)(value >> 8),
                (byte)(value & 0xFF)
            };

            return AppendCrc(
                frame);
        }

        private static byte[] BuildWrite10Frame(
            byte address,
            byte function,
            ushort register,
            ushort value)
        {
            byte[] frame =
            {
                address,
                function,

                (byte)(register >> 8),
                (byte)(register & 0xFF),

                0x00,
                0x01,

                0x02,

                (byte)(value >> 8),
                (byte)(value & 0xFF)
            };

            return AppendCrc(
                frame);
        }

        // ============================================================
        // 发送
        // ============================================================

        private byte[] SendFrame(
            byte[] frame,
            int minimumLength,
            DeviceProfile device)
        {
            TransportMode mode = GetTransportMode();
            bool connected =
                (mode == TransportMode.Rtu &&
                    _serialPort != null && _serialPort.IsOpen) ||
                (mode == TransportMode.Tcp &&
                    _tcpClient != null && _tcpClient.Connected) ||
                (mode == TransportMode.Udp && _udpClient != null);
            if (!connected)
                throw new InvalidOperationException(mode + " 未连接。");

            device.LastTxFrame =
                BytesToHex(frame);

            Log(
                "--------------------------------------------------");

            Log(
                "TX 发送：");

            Log(
                BytesToHex(frame));

            Log(
                "TX 长度：" +
                frame.Length);

            byte[] response;
            if (mode == TransportMode.Rtu)
                response = ExchangeSerial(frame, minimumLength);
            else if (mode == TransportMode.Tcp)
                response = ExchangeTcp(frame, minimumLength);
            else
                response = ExchangeUdp(frame);

            device.LastRxFrame =
                BytesToHex(
                    response);

            if (response.Length == 0)
            {
                Log(
                    "RX 接收：超时，没有返回数据。");

                throw new TimeoutException(
                    "设备没有返回数据。");
            }

            Log(
                "RX 接收：");

            Log(
                BytesToHex(
                    response));

            Log(
                "RX 长度：" +
                response.Length);

            if (response.Length >= 4)
            {
                bool crcOk =
                    CheckCrc(
                        response);

                Log(
                    "RX CRC：" +
                    (crcOk
                        ? "正确"
                        : "错误"));
            }

            return response;
        }

        private byte[] ExchangeSerial(byte[] frame, int minimumLength)
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.Write(frame, 0, frame.Length);

            List<byte> receive = new List<byte>();
            DateTime deadline = DateTime.Now.AddMilliseconds(1500);
            while (DateTime.Now < deadline)
            {
                int count = _serialPort.BytesToRead;
                if (count > 0)
                {
                    byte[] buffer = new byte[count];
                    int read = _serialPort.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        receive.AddRange(buffer.Take(read));
                    if (receive.Count >= minimumLength)
                        break;
                }
                Thread.Sleep(10);
            }
            return receive.ToArray();
        }

        private byte[] ExchangeTcp(byte[] frame, int minimumLength)
        {
            NetworkStream stream = _tcpClient.GetStream();
            while (stream.DataAvailable)
            {
                byte[] stale = new byte[Math.Max(1, _tcpClient.Available)];
                stream.Read(stale, 0, stale.Length);
            }

            stream.Write(frame, 0, frame.Length);
            stream.Flush();

            List<byte> receive = new List<byte>();
            DateTime deadline = DateTime.Now.AddMilliseconds(1500);
            while (DateTime.Now < deadline)
            {
                int count = _tcpClient.Available;
                if (count > 0)
                {
                    byte[] buffer = new byte[count];
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        receive.AddRange(buffer.Take(read));
                    if (receive.Count >= minimumLength)
                        break;
                }
                Thread.Sleep(10);
            }
            return receive.ToArray();
        }

        private byte[] ExchangeUdp(byte[] frame)
        {
            while (_udpClient.Available > 0)
            {
                IPEndPoint staleRemote = null;
                _udpClient.Receive(ref staleRemote);
            }

            _udpClient.Send(frame, frame.Length);
            try
            {
                IPEndPoint remote = null;
                return _udpClient.Receive(ref remote);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                    return new byte[0];
                throw;
            }
        }

        // ============================================================
        // 读取
        // ============================================================

        private bool ReadDevice(
            DeviceProfile device)
        {
            try
            {
                byte[] frame;
                byte expectedAddress = device.CurrentAddress;
                bool useCustomRead = !string.IsNullOrWhiteSpace(
                    device.CustomReadFrame);

                if (useCustomRead)
                {
                    Log(
                        "读取模式：自定义数据帧");

                    frame =
                        BuildCustomFrame(
                            device.CustomReadFrame,
                            device.AutoAppendCrc);
                }
                else
                {
                    Log(
                        "读取模式：标准 Modbus RTU");

                    frame =
                        BuildReadFrame(
                            device.CurrentAddress,
                            device.ReadFunctionCode,
                            device.RegisterAddress,
                            device.RegisterCount);
                }

                byte[] response =
                    SendFrame(
                        frame,
                        7,
                        device);

                if (useCustomRead)
                {
                    ValidateCustomResponse(response, expectedAddress);
                }
                else
                {
                    ValidateStandardResponse(
                        response,
                        expectedAddress,
                        device.ReadFunctionCode);
                }

                if (!useCustomRead && response.Length >= 5)
                {
                    int byteCount =
                        response[2];

                    Log(
                        "返回数据长度：" +
                        byteCount);

                    if (byteCount >= 2 &&
                        response.Length >= 5)
                    {
                        ushort value =
                            (ushort)(
                                (response[3] << 8) |
                                response[4]);

                        Log(
                            "第一个寄存器值：" +
                            FormatValue(value));

                        if (value != expectedAddress)
                        {
                            throw new InvalidDataException(string.Format(
                                "设备身份不匹配：当前选择地址为 {0}，读取值为 {1}。",
                                expectedAddress,
                                value));
                        }

                        Log("设备地址验证通过：" + expectedAddress);
                    }
                }

                ReadCustomRegisterItems(device);

                device.MarkStatus(
                    "读取成功");

                return true;
            }
            catch (Exception ex)
            {
                device.MarkStatus(
                    "读取失败：" +
                    ex.Message);

                Log(
                    "读取异常：" +
                    ex.Message);

                return false;
            }
            finally
            {
                RefreshGrid();
                RefreshSelectedDeviceEditor(device);
            }
        }

        // ============================================================
        // 修改
        // ============================================================

        private bool WriteDevice(
            DeviceProfile device)
        {
            try
            {
                byte[] frame;
                bool useCustomWrite = device.UseCustomWriteFrame &&
                    !string.IsNullOrWhiteSpace(device.CustomWriteFrame);

                if (useCustomWrite)
                {
                    Log(
                        "==================================================");

                    Log(
                        "修改模式：厂家自定义数据帧");

                    Log(
                        "程序不会重新拼接功能码、寄存器或地址。");

                    Log(
                        "厂家要求发送什么，就发送什么。");

                    frame =
                        BuildCustomFrame(
                            device.CustomWriteFrame,
                            device.AutoAppendCrc);
                }
                else
                {
                    Log(
                        "修改模式：标准 Modbus RTU");

                    Log(
                        string.Format(
                            "当前地址={0}, 新地址={1}, 寄存器=0x{2:X4}, 功能码=0x{3:X2}",
                            device.CurrentAddress,
                            device.NewAddress,
                            device.RegisterAddress,
                            device.WriteFunctionCode));

                    if (device.WriteFunctionCode == 6)
                    {
                        frame =
                            BuildWrite06Frame(
                                device.CurrentAddress,
                                device.WriteFunctionCode,
                                device.RegisterAddress,
                                device.NewAddress);
                    }
                    else if (
                        device.WriteFunctionCode == 16)
                    {
                        frame =
                            BuildWrite10Frame(
                                device.CurrentAddress,
                                device.WriteFunctionCode,
                                device.RegisterAddress,
                                device.NewAddress);
                    }
                    else
                    {
                        /*
                         * 非标准功能码仍然按照：
                         *
                         * 地址
                         * 功能码
                         * 寄存器
                         * 新地址
                         *
                         * 发送。
                         *
                         * 如果厂家结构不同，
                         * 请使用自定义数据帧。
                         */

                        frame =
                            BuildWrite06Frame(
                                device.CurrentAddress,
                                device.WriteFunctionCode,
                                device.RegisterAddress,
                                device.NewAddress);
                    }
                }

                byte[] response =
                    SendFrame(
                        frame,
                        useCustomWrite ? 4 : 8,
                        device);

                if (useCustomWrite)
                    ValidateCustomResponse(response, device.CurrentAddress);
                else
                    ValidateStandardResponse(
                        response,
                        device.CurrentAddress,
                        device.WriteFunctionCode);

                Log(
                    "修改命令发送完成。");

                byte previousAddress = device.CurrentAddress;
                device.CurrentAddress =
                    device.NewAddress;

                int customWriteCount =
                    WriteCustomRegisterItems(device);

                device.MarkStatus(
                    string.Format(
                        "修改命令已发送：{0} → {1}，自定义数据 {2} 项",
                        previousAddress,
                        device.NewAddress,
                        customWriteCount));

                return true;
            }
            catch (Exception ex)
            {
                device.MarkStatus(
                    "修改失败：" +
                    ex.Message);

                Log(
                    "修改异常：" +
                    ex.Message);

                return false;
            }
            finally
            {
                RefreshGrid();
                RefreshSelectedDeviceEditor(device);
            }
        }

        // ============================================================
        // 按钮
        // ============================================================

        private void ReadSelectedDevice(
            object sender,
            EventArgs e)
        {
            if (!SaveEditor())
                return;

            if (_selectedIndex < 0)
                return;

            Log(
                "========== 开始读取 ==========");

            DeviceProfile device = _devices[_selectedIndex];
            bool ok = ReadDevice(device);
            if (ok &&
                (device.CustomRegisterItems == null ||
                 device.CustomRegisterItems.All(item => item.CurrentValue.HasValue)))
            {
                MarkReadValidated(device);
                Log("读取验证通过，已允许修改当前设备。");
            }
            else
            {
                InvalidateReadValidation();
                Log("读取验证未通过，修改功能保持禁用。");
            }

            Log(
                "========== 读取结束 ==========");
        }

        private void WriteSelectedDevice(
            object sender,
            EventArgs e)
        {
            if (!SaveEditor())
                return;

            if (_selectedIndex < 0)
                return;

            DeviceProfile device =
                _devices[
                    _selectedIndex];

            if (!IsReadValidated(device))
            {
                InvalidateReadValidation();
                MessageBox.Show(
                    "当前设备尚未完成读取验证，请先读取当前设备及全部自定义项。",
                    "禁止修改",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Log(
                "========== 开始修改 ==========");

            bool ok =
                WriteDevice(
                    device);

            if (ok &&
                device.VerifyAfterWrite)
            {
                Log(
                    "开始执行修改后验证。");

                ok = VerifyNewAddress(device);
            }

            InvalidateReadValidation();

            Log(
                "========== 修改结束 ==========");

            MessageBox.Show(
                ok
                    ? "设备修改成功。"
                    : "设备修改失败，请查看通讯日志。",
                ok ? "修改成功" : "修改失败",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void ReadAllDevices(
            object sender,
            EventArgs e)
        {
            if (_selectedIndex >= 0 && !SaveEditor())
                return;

            foreach (
                DeviceProfile device
                in _devices)
            {
                ReadDevice(
                    device);

                Application.DoEvents();
            }
        }

        private void WriteAllDevices(
            object sender,
            EventArgs e)
        {
            if (_selectedIndex >= 0 && !SaveEditor())
                return;

            if (MessageBox.Show(
                "确定批量修改全部设备地址？",
                "确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)
                != DialogResult.Yes)
            {
                return;
            }

            foreach (
                DeviceProfile device
                in _devices)
            {
                if (WriteDevice(
                    device))
                {
                    if (device.VerifyAfterWrite)
                    {
                        VerifyNewAddress(
                            device);
                    }
                }

                Application.DoEvents();
            }
        }

        // ============================================================
        // 验证
        // ============================================================

        private bool VerifyNewAddress(
            DeviceProfile device)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(
                    device.CustomReadFrame))
                {
                    Log(
                        "验证使用自定义读取帧。");

                    byte[] custom =
                        BuildCustomFrame(
                            device.CustomReadFrame,
                            device.AutoAppendCrc);

                    byte[] response =
                        SendFrame(
                            custom,
                            4,
                            device);

                    if (response.Length > 0)
                    {
                        Log(
                            "自定义读取验证收到响应。");
                    }

                    device.MarkStatus(
                        "修改完成，验证收到响应");

                    return true;
                }

                byte[] frame =
                    BuildReadFrame(
                        device.NewAddress,
                        device.ReadFunctionCode,
                        device.RegisterAddress,
                        device.RegisterCount);

                byte[] result =
                    SendFrame(
                        frame,
                        7,
                        device);

                if (result.Length >= 5)
                {
                    ushort value =
                        (ushort)(
                            (result[3] << 8) |
                            result[4]);

                    Log(
                        "验证读取值：" +
                        FormatValue(value));

                    if (value ==
                        device.NewAddress)
                    {
                        device.MarkStatus(
                            "修改并验证成功");

                        Log(
                            "修改后验证成功。");

                        return true;
                    }
                }

                device.MarkStatus(
                    "修改成功，但验证未通过");

                return false;
            }
            catch (Exception ex)
            {
                device.MarkStatus(
                    "验证失败：" +
                    ex.Message);

                Log(
                    "验证失败：" +
                    ex.Message);

                return false;
            }
        }

        // ============================================================
        // 设备管理
        // ============================================================

        private void AddDevice(
            object sender,
            EventArgs e)
        {
            if (_selectedIndex >= 0 && !SaveEditor())
                return;

            int number =
                _devices.Count + 1;

            _devices.Add(
                new DeviceProfile
                {
                    Name =
                        "设备" + number
                });

            RefreshGrid();

            SelectDevice(
                _devices.Count - 1);

            Log(
                "新增设备：" +
                number);
        }

        private void DeleteDevice(
            object sender,
            EventArgs e)
        {
            if (_selectedIndex < 0)
                return;

            if (MessageBox.Show(
                "确定删除当前设备？",
                "确认",
                MessageBoxButtons.YesNo)
                != DialogResult.Yes)
            {
                return;
            }

            _devices.RemoveAt(
                _selectedIndex);

            if (_devices.Count == 0)
            {
                _devices.Add(
                    new DeviceProfile
                    {
                        Name = "设备1"
                    });
            }

            RefreshGrid();

            SelectDevice(
                Math.Min(
                    _selectedIndex,
                    _devices.Count - 1));
        }

        // ============================================================
        // 编辑保存
        // ============================================================

        private bool SaveEditor()
        {
            if (_selectedIndex < 0 ||
                _selectedIndex >= _devices.Count)
            {
                return false;
            }

            DeviceProfile device =
                _devices[
                    _selectedIndex];

            byte readFunction;

            byte writeFunction;

            ushort register;

            byte current;

            byte next;

            List<CustomRegisterItem> customItems;

            if (!TryParseFunction(
                _cmbReadFunction.Text,
                out readFunction))
            {
                MessageBox.Show(
                    "读取功能码无效。");

                return false;
            }

            if (!TryParseFunction(
                _cmbWriteFunction.Text,
                out writeFunction))
            {
                MessageBox.Show(
                    "写入功能码无效。");

                return false;
            }

            if (!TryParseUShort(
                _txtRegister.Text,
                out register))
            {
                MessageBox.Show(
                    "寄存器地址无效。\r\n例如：00D0、0x00D0、208");

                return false;
            }

            if (!TryParseAddress(
                _txtCurrentAddress.Text,
                out current))
            {
                MessageBox.Show(
                    "当前地址必须为 1～247。");

                return false;
            }

            if (!TryParseAddress(
                _txtNewAddress.Text,
                out next))
            {
                MessageBox.Show(
                    "修改地址必须为 1～247。");

                return false;
            }

            if (!TryReadCustomDataGrid(out customItems))
                return false;

            device.Name =
                _txtName.Text.Trim();

            device.ReadFunctionCode =
                readFunction;

            device.WriteFunctionCode =
                writeFunction;

            device.RegisterAddress =
                register;

            device.CurrentAddress =
                current;

            device.NewAddress =
                next;

            device.CustomReadFrame =
                _txtCustomReadFrame.Text.Trim();

            device.CustomWriteFrame =
                _txtCustomWriteFrame.Text.Trim();

            device.UseCustomWriteFrame =
                _chkCustomWriteFrame.Checked;

            device.AutoAppendCrc =
                _chkAutoCrc.Checked;

            device.VerifyAfterWrite =
                _chkVerify.Checked;

            device.CustomRegisterItems =
                customItems;

            return true;
        }

        private bool TryReadCustomDataGrid(
            out List<CustomRegisterItem> items)
        {
            items = new List<CustomRegisterItem>();
            _customDataGrid.EndEdit();

            foreach (DataGridViewRow row in _customDataGrid.Rows)
            {
                string name = Convert.ToString(
                    row.Cells["ItemName"].Value).Trim();
                string registerText = Convert.ToString(
                    row.Cells["ItemRegister"].Value).Trim();
                string currentText = Convert.ToString(
                    row.Cells["ItemCurrent"].Value).Trim();
                string targetText = Convert.ToString(
                    row.Cells["ItemTarget"].Value).Trim();

                ushort register;
                if (!TryParseUShort(registerText, out register))
                {
                    MessageBox.Show(string.Format(
                        "自定义修改数据第 {0} 行的寄存器地址无效。",
                        row.Index + 1));
                    return false;
                }

                ushort current = 0;
                if (!string.IsNullOrWhiteSpace(currentText) &&
                    !TryParseUShort(currentText, out current))
                {
                    MessageBox.Show(string.Format(
                        "自定义修改数据第 {0} 行的当前值无效。",
                        row.Index + 1));
                    return false;
                }

                ushort target = 0;
                if (!string.IsNullOrWhiteSpace(targetText) &&
                    !TryParseUShort(targetText, out target))
                {
                    MessageBox.Show(string.Format(
                        "自定义修改数据第 {0} 行的目标值无效。",
                        row.Index + 1));
                    return false;
                }

                items.Add(new CustomRegisterItem
                {
                    Name = string.IsNullOrWhiteSpace(name)
                        ? "自定义项" + (row.Index + 1)
                        : name,
                    RegisterAddress = register,
                    CurrentValue = string.IsNullOrWhiteSpace(currentText)
                        ? (ushort?)null
                        : current,
                    TargetValue = string.IsNullOrWhiteSpace(targetText)
                        ? (ushort?)null
                        : target
                });
            }

            return true;
        }

        // ============================================================
        // Grid
        // ============================================================

        private void RefreshGrid()
        {
            if (_grid == null)
                return;

            _grid.Rows.Clear();

            foreach (
                DeviceProfile device
                in _devices)
            {
                int row =
                    _grid.Rows.Add();

                _grid.Rows[row]
                    .Cells["Name"]
                    .Value =
                    device.Name;

                _grid.Rows[row]
                    .Cells["Register"]
                    .Value =
                    device.RegisterAddress
                        .ToString("X4");

                _grid.Rows[row]
                    .Cells["Current"]
                    .Value =
                    FormatValue(
                        device.CurrentAddress);

                _grid.Rows[row]
                    .Cells["New"]
                    .Value =
                    FormatValue(
                        device.NewAddress);

                _grid.Rows[row]
                    .Cells["Status"]
                    .Value =
                    device.LastResult;
            }
        }

        private void Grid_SelectionChanged(
            object sender,
            EventArgs e)
        {
            if (_suppressDeviceSelection)
                return;

            if (_grid.SelectedRows.Count == 0)
                return;

            int index =
                _grid.SelectedRows[0].Index;

            if (index < 0 ||
                index >= _devices.Count)
                return;

            if (index == _selectedIndex)
                return;

            int previousIndex = _selectedIndex;
            if (previousIndex >= 0 && !SaveEditor())
            {
                SelectDevice(previousIndex);
                return;
            }

            InvalidateReadValidation();
            _selectedIndex =
                index;

            LoadDevice(
                _devices[index]);
        }

        private void SelectDevice(
            int index)
        {
            if (index < 0 ||
                index >= _grid.Rows.Count)
                return;

            if (index != _selectedIndex)
                InvalidateReadValidation();

            _suppressDeviceSelection = true;
            _grid.ClearSelection();

            _grid.Rows[index]
                .Selected = true;
            _grid.CurrentCell = _grid.Rows[index].Cells[0];
            _suppressDeviceSelection = false;

            _selectedIndex =
                index;

            LoadDevice(
                _devices[index]);
        }

        private void LoadDevice(
            DeviceProfile device)
        {
            _loadingDevice = true;
            try
            {
            _txtName.Text =
                device.Name;

            _txtRegister.Text =
                device.RegisterAddress
                    .ToString("X4");

            _txtCurrentAddress.Text =
                FormatValue(
                    device.CurrentAddress);

            _txtNewAddress.Text =
                FormatValue(
                    device.NewAddress);

            _cmbReadFunction.Text =
                device.ReadFunctionCode
                    .ToString("X2");

            _cmbWriteFunction.Text =
                device.WriteFunctionCode
                    .ToString("X2");

            _txtCustomReadFrame.Text =
                device.CustomReadFrame;

            _txtCustomWriteFrame.Text =
                device.CustomWriteFrame;

            _chkCustomWriteFrame.Checked =
                device.UseCustomWriteFrame;

            _chkAutoCrc.Checked =
                device.AutoAppendCrc;

            _chkVerify.Checked =
                device.VerifyAfterWrite;

            if (device.CustomRegisterItems == null)
                device.CustomRegisterItems = new List<CustomRegisterItem>();

            _customDataGrid.Rows.Clear();
            foreach (CustomRegisterItem item in device.CustomRegisterItems)
            {
                int row = _customDataGrid.Rows.Add();
                _customDataGrid.Rows[row].Cells["ItemName"].Value = item.Name;
                _customDataGrid.Rows[row].Cells["ItemRegister"].Value =
                    "0x" + item.RegisterAddress.ToString("X4");
                _customDataGrid.Rows[row].Cells["ItemCurrent"].Value =
                    item.CurrentValue.HasValue
                        ? FormatValue(item.CurrentValue.Value)
                        : "";
                _customDataGrid.Rows[row].Cells["ItemTarget"].Value =
                    item.TargetValue.HasValue
                        ? FormatValue(item.TargetValue.Value)
                        : "";
            }
            }
            finally
            {
                _loadingDevice = false;
            }
        }

        private void ReadCustomRegisterItems(DeviceProfile device)
        {
            if (device.CustomRegisterItems == null ||
                device.CustomRegisterItems.Count == 0)
                return;

            foreach (CustomRegisterItem item in device.CustomRegisterItems)
            {
                byte[] frame = BuildReadFrame(
                    device.CurrentAddress,
                    device.ReadFunctionCode,
                    item.RegisterAddress,
                    1);
                byte[] response = SendFrame(frame, 7, device);
                ValidateStandardResponse(
                    response,
                    device.CurrentAddress,
                    device.ReadFunctionCode);
                if (response[2] < 2)
                    throw new InvalidDataException(
                        "自定义项目“" + item.Name + "”读取响应长度不足。");

                item.CurrentValue = (ushort)(
                    (response[3] << 8) | response[4]);
                Log(string.Format(
                    "自定义项目读取：{0}，寄存器=0x{1:X4}，当前值={2}",
                    item.Name,
                    item.RegisterAddress,
                    item.CurrentValue.Value));
            }
        }

        private int WriteCustomRegisterItems(DeviceProfile device)
        {
            if (device.CustomRegisterItems == null)
                return 0;

            int written = 0;
            foreach (CustomRegisterItem item in device.CustomRegisterItems)
            {
                if (!item.TargetValue.HasValue ||
                    (item.CurrentValue.HasValue &&
                     item.CurrentValue.Value == item.TargetValue.Value))
                {
                    continue;
                }

                byte[] frame;
                if (device.WriteFunctionCode == 16)
                {
                    frame = BuildWrite10Frame(
                        device.CurrentAddress,
                        device.WriteFunctionCode,
                        item.RegisterAddress,
                        item.TargetValue.Value);
                }
                else
                {
                    frame = BuildWrite06Frame(
                        device.CurrentAddress,
                        device.WriteFunctionCode,
                        item.RegisterAddress,
                        item.TargetValue.Value);
                }

                byte[] response = SendFrame(frame, 8, device);
                ValidateStandardResponse(
                    response,
                    device.CurrentAddress,
                    device.WriteFunctionCode);
                Log(string.Format(
                    "自定义项目修改：{0}，寄存器=0x{1:X4}，{2} → {3}",
                    item.Name,
                    item.RegisterAddress,
                    item.CurrentValue.HasValue
                        ? item.CurrentValue.Value.ToString()
                        : "未知",
                    item.TargetValue.Value));
                item.CurrentValue = item.TargetValue;
                written++;
            }
            return written;
        }

        private static void ThrowIfModbusException(byte[] response)
        {
            if (response.Length >= 3 && (response[1] & 0x80) != 0)
            {
                throw new InvalidDataException(string.Format(
                    "设备返回 Modbus 异常码：0x{0:X2}",
                    response[2]));
            }
        }

        private static void ValidateStandardResponse(
            byte[] response,
            byte expectedAddress,
            byte expectedFunction)
        {
            if (response == null || response.Length < 5)
                throw new InvalidDataException("设备响应长度不足。");

            if (!CheckCrc(response))
                throw new InvalidDataException("设备响应 CRC 校验失败。");

            if (response[0] != expectedAddress)
            {
                throw new InvalidDataException(string.Format(
                    "响应设备地址不匹配：期望 {0}，实际 {1}。",
                    expectedAddress,
                    response[0]));
            }

            ThrowIfModbusException(response);
            if (response[1] != expectedFunction)
            {
                throw new InvalidDataException(string.Format(
                    "响应功能码不匹配：期望 0x{0:X2}，实际 0x{1:X2}。",
                    expectedFunction,
                    response[1]));
            }
        }

        private static void ValidateCustomResponse(
            byte[] response,
            byte expectedAddress)
        {
            if (response == null || response.Length < 4)
                throw new InvalidDataException("自定义读取响应长度不足。");
            if (!CheckCrc(response))
                throw new InvalidDataException("自定义读取响应 CRC 校验失败。");
            if (response[0] != expectedAddress)
            {
                throw new InvalidDataException(string.Format(
                    "响应设备地址不匹配：期望 {0}，实际 {1}。",
                    expectedAddress,
                    response[0]));
            }
            ThrowIfModbusException(response);
        }

        private void RefreshSelectedDeviceEditor(DeviceProfile device)
        {
            if (_selectedIndex >= 0 &&
                _selectedIndex < _devices.Count &&
                object.ReferenceEquals(_devices[_selectedIndex], device))
            {
                LoadDevice(device);
            }
        }

        // ============================================================
        // 导入导出
        // ============================================================

        private void SaveDefaultDevice(
            object sender,
            EventArgs e)
        {
            if (!SaveEditor())
                return;

            try
            {
                string path = WriteDefaultDevices();

                int index = _selectedIndex;
                RefreshGrid();
                SelectDevice(index);
                Log(string.Format(
                    "全部设备已保存为默认配置：{0}（{1} 台设备）",
                    path,
                    _devices.Count));
                MessageBox.Show(
                    "默认配置已保存。\r\n" + path,
                    "保存成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("默认配置保存失败：" + ex.Message);
                MessageBox.Show(
                    "默认配置保存失败：\r\n" + ex.Message,
                    "保存失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string WriteDefaultDevices()
        {
            string path = GetDefaultProfilePath();
            var serializer = new DataContractJsonSerializer(
                typeof(List<DeviceProfile>));
            using (FileStream stream = File.Create(path))
            {
                serializer.WriteObject(
                    stream,
                    _devices);
            }
            return path;
        }

        private void ExportDevices(
            object sender,
            EventArgs e)
        {
            if (!SaveEditor())
                return;

            using (
                SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Filter =
                    "JSON 文件 (*.json)|*.json";

                dialog.FileName =
                    "ModbusAddressDevices.json";

                if (dialog.ShowDialog()
                    != DialogResult.OK)
                    return;

                var serializer =
                    new DataContractJsonSerializer(
                        typeof(
                            List<DeviceProfile>));

                using (
                    FileStream stream =
                    File.Create(
                        dialog.FileName))
                {
                    serializer.WriteObject(
                        stream,
                        _devices);
                }

                Log(
                    "设备配置导出成功：" +
                    dialog.FileName);
            }
        }

        private void ImportDevices(
            object sender,
            EventArgs e)
        {
            using (
                OpenFileDialog dialog =
                new OpenFileDialog())
            {
                dialog.Filter =
                    "JSON 文件 (*.json)|*.json";

                if (dialog.ShowDialog()
                    != DialogResult.OK)
                    return;

                try
                {
                    var serializer =
                        new DataContractJsonSerializer(
                            typeof(
                                List<DeviceProfile>));

                    List<DeviceProfile> result;

                    using (
                        FileStream stream =
                        File.OpenRead(
                            dialog.FileName))
                    {
                        result =
                            serializer.ReadObject(
                                stream)
                            as List<DeviceProfile>;
                    }

                    if (result == null)
                        throw new Exception(
                            "配置文件格式错误。");

                    _devices.Clear();

                    _devices.AddRange(
                        result);

                    if (_devices.Count == 0)
                    {
                        _devices.Add(
                            new DeviceProfile
                            {
                                Name = "设备1"
                            });
                    }

                    RefreshGrid();

                    SelectDevice(0);

                    Log(
                        "设备配置导入成功：" +
                        dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "导入失败：\r\n" +
                        ex.Message);
                }
            }
        }

        // ============================================================
        // 数据格式
        // ============================================================

        private void DisplayFormatChanged(
            object sender,
            EventArgs e)
        {
            if (_radioHex.Checked)
            {
                _displayFormat =
                    DisplayFormat.Hex;
            }
            else if (_radioBinary.Checked)
            {
                _displayFormat =
                    DisplayFormat.Binary;
            }
            else
            {
                _displayFormat =
                    DisplayFormat.Integer;
            }

            RefreshGrid();

            if (_selectedIndex >= 0 &&
                _selectedIndex < _devices.Count &&
                SaveEditor())
            {
                LoadDevice(_devices[_selectedIndex]);
            }
        }

        private string FormatValue(
            ushort value)
        {
            switch (_displayFormat)
            {
                case DisplayFormat.Hex:

                    return "0x" +
                        value.ToString("X4");

                case DisplayFormat.Binary:

                    return Convert.ToString(
                        value,
                        2)
                        .PadLeft(
                            16,
                            '0');

                default:

                    return value.ToString();
            }
        }

        private string FormatValue(
            byte value)
        {
            switch (_displayFormat)
            {
                case DisplayFormat.Hex:

                    return "0x" +
                        value.ToString("X2");

                case DisplayFormat.Binary:

                    return Convert.ToString(
                        value,
                        2)
                        .PadLeft(
                            8,
                            '0');

                default:

                    return value.ToString();
            }
        }

        // ============================================================
        // 输入解析
        // ============================================================

        private static bool TryParseFunction(
            string text,
            out byte value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(
                text))
                return false;

            text =
                text.Trim();

            int number;

            if (text.StartsWith(
                "0x",
                StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(
                    text.Substring(2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out number))
                    return false;
            }
            else
            {
                if (!int.TryParse(
                    text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out number))
                {
                    // 允许输入 A-F 的十六进制
                    if (!int.TryParse(
                        text,
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture,
                        out number))
                        return false;
                }
            }

            if (number < 1 ||
                number > 255)
                return false;

            value =
                (byte)number;

            return true;
        }

        private static bool TryParseUShort(
            string text,
            out ushort value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(
                text))
                return false;

            text =
                text.Trim();

            if ((text.Length == 8 || text.Length == 16) &&
                text.All(c => c == '0' || c == '1'))
            {
                try
                {
                    value = Convert.ToUInt16(text, 2);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (text.StartsWith(
                "0x",
                StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(
                    text.Substring(2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            if (text.Any(
                c =>
                    (c >= 'A' &&
                     c <= 'F') ||
                    (c >= 'a' &&
                     c <= 'f')))
            {
                return ushort.TryParse(
                    text,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            return ushort.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static bool TryParseAddress(
            string text,
            out byte value)
        {
            value = 0;

            ushort number;

            if (!TryParseUShort(
                text,
                out number))
                return false;

            if (number < 1 ||
                number > 247)
                return false;

            value =
                (byte)number;

            return true;
        }

        // ============================================================
        // 日志
        // ============================================================

        private void Log(
            string text)
        {
            if (_txtLog == null)
                return;

            _txtLog.AppendText(
                string.Format(
                    "[{0}] {1}\r\n",
                    DateTime.Now.ToString(
                        "HH:mm:ss.fff"),
                    text));

            _txtLog.SelectionStart =
                _txtLog.TextLength;

            _txtLog.ScrollToCaret();
        }

        private static string BytesToHex(
            byte[] data)
        {
            if (data == null ||
                data.Length == 0)
                return "";

            return string.Join(
                " ",
                data.Select(
                    b =>
                        b.ToString("X2")));
        }

        // ============================================================
        // 关闭
        // ============================================================

        private void MainForm_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            Disconnect();
        }
    }
}
