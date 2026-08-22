using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ModbusAddressTool
{
    public class MainForm : Form
    {
        private SerialPort _serialPort;

        private readonly List<DeviceProfile> _devices =
            new List<DeviceProfile>();

        private DataGridView _grid;

        private ComboBox _cmbPort;
        private ComboBox _cmbBaud;
        private ComboBox _cmbParity;
        private ComboBox _cmbDataBits;
        private ComboBox _cmbStopBits;

        private ComboBox _cmbReadFunction;
        private ComboBox _cmbWriteFunction;

        private TextBox _txtName;
        private TextBox _txtRegister;
        private TextBox _txtCurrentAddress;
        private TextBox _txtNewAddress;

        private TextBox _txtCustomReadFrame;
        private TextBox _txtCustomWriteFrame;

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
        private Button _btnReadAll;
        private Button _btnWriteAll;
        private Button _btnImport;
        private Button _btnExport;

        private RichTextBox _txtLog;

        private int _selectedIndex = -1;

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
            Text =
                "Modbus RTU RS485 地址修改工具";

            Width = 1400;

            Height = 900;

            MinimumSize =
                new System.Drawing.Size(
                    1100,
                    700);

            StartPosition =
                FormStartPosition.CenterScreen;

            FormClosing +=
                MainForm_FormClosing;
        }

        // ============================================================
        // 串口
        // ============================================================

        private void InitializeSerialPanel()
        {
            var panel =
                new TableLayoutPanel();

            panel.Dock =
                DockStyle.Top;

            panel.Height =
                105;

            panel.Padding =
                new Padding(8);

            panel.ColumnCount = 12;

            panel.RowCount = 2;

            for (int i = 0; i < 12; i++)
            {
                panel.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        8.333f));
            }

            Controls.Add(panel);

            AddLabel(
                panel,
                "串口",
                0,
                0);

            _cmbPort =
                new ComboBox();

            _cmbPort.Dock =
                DockStyle.Fill;

            _cmbPort.DropDownStyle =
                ComboBoxStyle.DropDownList;

            panel.Controls.Add(
                _cmbPort,
                1,
                0);

            _btnRefresh =
                new Button();

            _btnRefresh.Text =
                "刷新";

            _btnRefresh.Dock =
                DockStyle.Fill;

            _btnRefresh.Click +=
                delegate
                {
                    RefreshPorts();
                };

            panel.Controls.Add(
                _btnRefresh,
                2,
                0);

            AddLabel(
                panel,
                "波特率",
                3,
                0);

            _cmbBaud =
                new ComboBox();

            _cmbBaud.Dock =
                DockStyle.Fill;

            _cmbBaud.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cmbBaud.Items.AddRange(
                new object[]
                {
                    "1200",
                    "2400",
                    "4800",
                    "9600",
                    "19200",
                    "38400",
                    "57600",
                    "115200"
                });

            _cmbBaud.SelectedItem =
                "9600";

            panel.Controls.Add(
                _cmbBaud,
                4,
                0);

            AddLabel(
                panel,
                "校验位",
                5,
                0);

            _cmbParity =
                new ComboBox();

            _cmbParity.Dock =
                DockStyle.Fill;

            _cmbParity.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cmbParity.Items.AddRange(
                new object[]
                {
                    "None",
                    "Even",
                    "Odd",
                    "Mark",
                    "Space"
                });

            _cmbParity.SelectedItem =
                "None";

            panel.Controls.Add(
                _cmbParity,
                6,
                0);

            AddLabel(
                panel,
                "数据位",
                7,
                0);

            _cmbDataBits =
                new ComboBox();

            _cmbDataBits.Dock =
                DockStyle.Fill;

            _cmbDataBits.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cmbDataBits.Items.AddRange(
                new object[]
                {
                    "8",
                    "7"
                });

            _cmbDataBits.SelectedItem =
                "8";

            panel.Controls.Add(
                _cmbDataBits,
                8,
                0);

            AddLabel(
                panel,
                "停止位",
                9,
                0);

            _cmbStopBits =
                new ComboBox();

            _cmbStopBits.Dock =
                DockStyle.Fill;

            _cmbStopBits.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cmbStopBits.Items.AddRange(
                new object[]
                {
                    "1",
                    "1.5",
                    "2"
                });

            _cmbStopBits.SelectedItem =
                "1";

            panel.Controls.Add(
                _cmbStopBits,
                10,
                0);

            var buttons =
                new FlowLayoutPanel();

            buttons.Dock =
                DockStyle.Fill;

            _btnConnect =
                CreateButton(
                    "连接",
                    Connect);

            _btnDisconnect =
                CreateButton(
                    "断开",
                    Disconnect);

            buttons.Controls.Add(
                _btnConnect);

            buttons.Controls.Add(
                _btnDisconnect);

            panel.Controls.Add(
                buttons,
                11,
                0);

            // 第二行
            AddLabel(
                panel,
                "读取功能码",
                0,
                1);

            _cmbReadFunction =
                CreateFunctionCombo(
                    "03");

            panel.Controls.Add(
                _cmbReadFunction,
                1,
                1);

            AddLabel(
                panel,
                "写入功能码",
                2,
                1);

            _cmbWriteFunction =
                CreateFunctionCombo(
                    "06");

            panel.Controls.Add(
                _cmbWriteFunction,
                3,
                1);

            var info =
                new Label();

            info.Text =
                "功能码可直接输入十进制/十六进制";

            info.Dock =
                DockStyle.Fill;

            info.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            panel.Controls.Add(
                info,
                4,
                1);

            panel.SetColumnSpan(
                info,
                5);
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
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                column,
                row);
        }

        // ============================================================
        // 主设备区域
        // ============================================================

        private void InitializeDevicePanel()
        {
            var main =
                new TableLayoutPanel();

            main.Dock =
                DockStyle.Fill;

            main.ColumnCount = 1;

            main.RowCount = 5;

            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    42));

            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    145));

            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    120));

            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    50));

            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100));

            Controls.Add(main);

            // ========================================================
            // 设备列表
            // ========================================================

            _grid =
                new DataGridView();

            _grid.Dock =
                DockStyle.Fill;

            _grid.AllowUserToAddRows =
                false;

            _grid.AllowUserToDeleteRows =
                false;

            _grid.MultiSelect =
                false;

            _grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _grid.AutoGenerateColumns =
                false;

            AddColumn(
                "Name",
                "名称",
                130);

            AddColumn(
                "ReadFunction",
                "读取功能码",
                90);

            AddColumn(
                "WriteFunction",
                "写入功能码",
                90);

            AddColumn(
                "Register",
                "寄存器",
                90);

            AddColumn(
                "Current",
                "当前地址",
                90);

            AddColumn(
                "New",
                "修改地址",
                90);

            AddColumn(
                "Custom",
                "自定义帧",
                90);

            AddColumn(
                "Status",
                "状态",
                260);

            _grid.SelectionChanged +=
                Grid_SelectionChanged;

            main.Controls.Add(
                _grid,
                0,
                0);

            // ========================================================
            // 编辑
            // ========================================================

            var editor =
                new FlowLayoutPanel();

            editor.Dock =
                DockStyle.Fill;

            editor.Padding =
                new Padding(8);

            editor.WrapContents =
                false;

            AddEditorLabel(
                editor,
                "名称");

            _txtName =
                CreateTextBox(130);

            editor.Controls.Add(
                _txtName);

            AddEditorLabel(
                editor,
                "寄存器");

            _txtRegister =
                CreateTextBox(90);

            editor.Controls.Add(
                _txtRegister);

            AddEditorLabel(
                editor,
                "当前地址");

            _txtCurrentAddress =
                CreateTextBox(70);

            editor.Controls.Add(
                _txtCurrentAddress);

            AddEditorLabel(
                editor,
                "修改地址");

            _txtNewAddress =
                CreateTextBox(70);

            editor.Controls.Add(
                _txtNewAddress);

            main.Controls.Add(
                editor,
                0,
                1);

            // ========================================================
            // 自定义数据帧
            // ========================================================

            var framePanel =
                new TableLayoutPanel();

            framePanel.Dock =
                DockStyle.Fill;

            framePanel.Padding =
                new Padding(8);

            framePanel.ColumnCount =
                4;

            framePanel.RowCount =
                3;

            framePanel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    130));

            framePanel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            framePanel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    130));

            framePanel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            framePanel.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    33));

            framePanel.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    33));

            framePanel.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    34));

            framePanel.Controls.Add(
                new Label
                {
                    Text = "自定义读取帧",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleLeft
                },
                0,
                0);

            _txtCustomReadFrame =
                new TextBox();

            _txtCustomReadFrame.Dock =
                DockStyle.Fill;

            _txtCustomReadFrame.Font =
                new System.Drawing.Font(
                    "Consolas",
                    10);

            framePanel.Controls.Add(
                _txtCustomReadFrame,
                1,
                0);

            framePanel.Controls.Add(
                new Label
                {
                    Text = "自定义修改帧",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleLeft
                },
                2,
                0);

            _txtCustomWriteFrame =
                new TextBox();

            _txtCustomWriteFrame.Dock =
                DockStyle.Fill;

            _txtCustomWriteFrame.Font =
                new System.Drawing.Font(
                    "Consolas",
                    10);

            framePanel.Controls.Add(
                _txtCustomWriteFrame,
                3,
                0);

            _chkCustomWriteFrame =
                new CheckBox();

            _chkCustomWriteFrame.Text =
                "启用自定义修改帧（填写后完全按厂家数据发送）";

            _chkCustomWriteFrame.Dock =
                DockStyle.Fill;

            framePanel.Controls.Add(
                _chkCustomWriteFrame,
                0,
                1);

            framePanel.SetColumnSpan(
                _chkCustomWriteFrame,
                2);

            _chkAutoCrc =
                new CheckBox();

            _chkAutoCrc.Text =
                "自动追加 Modbus CRC16";

            _chkAutoCrc.Checked =
                true;

            _chkAutoCrc.Dock =
                DockStyle.Fill;

            framePanel.Controls.Add(
                _chkAutoCrc,
                2,
                1);

            _chkVerify =
                new CheckBox();

            _chkVerify.Text =
                "修改后自动验证";

            _chkVerify.Checked =
                true;

            _chkVerify.Dock =
                DockStyle.Fill;

            framePanel.Controls.Add(
                _chkVerify,
                3,
                1);

            var hint =
                new Label();

            hint.Text =
                "数据帧示例：01 41 00 D0 02    |    已含CRC时取消“自动追加CRC”";

            hint.Dock =
                DockStyle.Fill;

            hint.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            framePanel.Controls.Add(
                hint,
                0,
                2);

            framePanel.SetColumnSpan(
                hint,
                4);

            main.Controls.Add(
                framePanel,
                0,
                2);

            // ========================================================
            // 按钮
            // ========================================================

            var buttons =
                new FlowLayoutPanel();

            buttons.Dock =
                DockStyle.Fill;

            buttons.Padding =
                new Padding(8);

            _btnAdd =
                CreateButton(
                    "添加设备",
                    AddDevice);

            _btnDelete =
                CreateButton(
                    "删除设备",
                    DeleteDevice);

            _btnRead =
                CreateButton(
                    "读取当前地址",
                    ReadSelectedDevice);

            _btnWrite =
                CreateButton(
                    "修改地址",
                    WriteSelectedDevice);

            _btnReadAll =
                CreateButton(
                    "读取全部",
                    ReadAllDevices);

            _btnWriteAll =
                CreateButton(
                    "修改全部",
                    WriteAllDevices);

            _btnImport =
                CreateButton(
                    "导入",
                    ImportDevices);

            _btnExport =
                CreateButton(
                    "导出",
                    ExportDevices);

            buttons.Controls.Add(_btnAdd);
            buttons.Controls.Add(_btnDelete);
            buttons.Controls.Add(_btnRead);
            buttons.Controls.Add(_btnWrite);
            buttons.Controls.Add(_btnReadAll);
            buttons.Controls.Add(_btnWriteAll);
            buttons.Controls.Add(_btnImport);
            buttons.Controls.Add(_btnExport);

            main.Controls.Add(
                buttons,
                0,
                3);

            // ========================================================
            // 数据显示
            // ========================================================

            var display =
                new FlowLayoutPanel();

            display.Dock =
                DockStyle.Fill;

            display.Padding =
                new Padding(8);

            display.Controls.Add(
                new Label
                {
                    Text = "数据显示格式：",
                    AutoSize = true,
                    Padding =
                        new Padding(0, 6, 8, 0)
                });

            _radioInteger =
                new RadioButton
                {
                    Text = "整数",
                    Checked = true,
                    AutoSize = true
                };

            _radioHex =
                new RadioButton
                {
                    Text = "十六进制",
                    AutoSize = true
                };

            _radioBinary =
                new RadioButton
                {
                    Text = "二进制",
                    AutoSize = true
                };

            _radioInteger.CheckedChanged +=
                DisplayFormatChanged;

            _radioHex.CheckedChanged +=
                DisplayFormatChanged;

            _radioBinary.CheckedChanged +=
                DisplayFormatChanged;

            display.Controls.Add(
                _radioInteger);

            display.Controls.Add(
                _radioHex);

            display.Controls.Add(
                _radioBinary);

            main.Controls.Add(
                display,
                0,
                4);
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

            column.Width =
                width;

            column.ReadOnly =
                true;

            _grid.Columns.Add(
                column);
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
                Height = 25
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
                110;

            button.Height =
                32;

            button.Click +=
                handler;

            return button;
        }

        // ============================================================
        // 日志窗口
        // ============================================================

        private void InitializeLogPanel()
        {
            var group =
                new GroupBox();

            group.Text =
                "通讯日志 / 调试信息";

            group.Dock =
                DockStyle.Bottom;

            group.Height =
                210;

            _txtLog =
                new RichTextBox();

            _txtLog.Dock =
                DockStyle.Fill;

            _txtLog.ReadOnly =
                true;

            _txtLog.WordWrap =
                false;

            _txtLog.Font =
                new System.Drawing.Font(
                    "Consolas",
                    9);

            group.Controls.Add(
                _txtLog);

            Controls.Add(
                group);
        }

        // ============================================================
        // 初始化设备
        // ============================================================

        private void InitializeDevices()
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

            RefreshGrid();

            SelectDevice(0);
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

        private void Connect()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    _cmbPort.Text))
                {
                    MessageBox.Show(
                        "请选择串口。");

                    return;
                }

                Disconnect();

                int baud =
                    int.Parse(
                        _cmbBaud.Text);

                Parity parity =
                    ParseParity(
                        _cmbParity.Text);

                int dataBits =
                    int.Parse(
                        _cmbDataBits.Text);

                StopBits stopBits =
                    ParseStopBits(
                        _cmbStopBits.Text);

                _serialPort =
                    new SerialPort(
                        _cmbPort.Text,
                        baud,
                        parity,
                        dataBits,
                        stopBits);

                _serialPort.ReadTimeout =
                    1500;

                _serialPort.WriteTimeout =
                    1500;

                _serialPort.Open();

                SetConnectedState(
                    true);

                Log(
                    "串口连接成功：");

                Log(
                    string.Format(
                        "PORT={0}, Baud={1}, Parity={2}, DataBits={3}, StopBits={4}",
                        _cmbPort.Text,
                        baud,
                        parity,
                        dataBits,
                        stopBits));
            }
            catch (Exception ex)
            {
                Log(
                    "串口连接失败：" +
                    ex.Message);

                MessageBox.Show(
                    "连接失败：\r\n" +
                    ex.Message);
            }
        }

        private void Disconnect(
            object sender = null,
            EventArgs e = null)
        {
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
            }
            catch
            {
            }

            _serialPort =
                null;

            SetConnectedState(
                false);

            Log(
                "串口已断开。");
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

            _btnRead.Enabled =
                connected;

            _btnWrite.Enabled =
                connected;

            _btnReadAll.Enabled =
                connected;

            _btnWriteAll.Enabled =
                connected;
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
            byte newAddress)
        {
            byte[] frame =
            {
                address,
                function,

                (byte)(register >> 8),
                (byte)(register & 0xFF),

                0x00,
                newAddress
            };

            return AppendCrc(
                frame);
        }

        private static byte[] BuildWrite10Frame(
            byte address,
            byte function,
            ushort register,
            byte newAddress)
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

                0x00,
                newAddress
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
            if (_serialPort == null ||
                !_serialPort.IsOpen)
            {
                throw new InvalidOperationException(
                    "串口未连接。");
            }

            _serialPort.DiscardInBuffer();

            _serialPort.DiscardOutBuffer();

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

            _serialPort.Write(
                frame,
                0,
                frame.Length);

            List<byte> receive =
                new List<byte>();

            DateTime deadline =
                DateTime.Now.AddMilliseconds(
                    1500);

            while (
                DateTime.Now <
                deadline)
            {
                int count =
                    _serialPort.BytesToRead;

                if (count > 0)
                {
                    byte[] buffer =
                        new byte[count];

                    int read =
                        _serialPort.Read(
                            buffer,
                            0,
                            buffer.Length);

                    if (read > 0)
                    {
                        receive.AddRange(
                            buffer);
                    }

                    if (receive.Count >=
                        minimumLength)
                    {
                        break;
                    }
                }

                Thread.Sleep(10);
            }

            byte[] response =
                receive.ToArray();

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

        // ============================================================
        // 读取
        // ============================================================

        private bool ReadDevice(
            DeviceProfile device)
        {
            try
            {
                byte[] frame;

                if (!string.IsNullOrWhiteSpace(
                    device.CustomReadFrame))
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
                        5,
                        device);

                if (response.Length >= 5)
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

                        if (value >= 1 &&
                            value <= 247)
                        {
                            device.CurrentAddress =
                                (byte)value;

                            Log(
                                "识别当前设备地址：" +
                                device.CurrentAddress);
                        }
                    }
                }

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

                if (device.UseCustomWriteFrame &&
                    !string.IsNullOrWhiteSpace(
                        device.CustomWriteFrame))
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
                        4,
                        device);

                if (response.Length >= 3 &&
                    (response[1] & 0x80) != 0)
                {
                    byte exceptionCode =
                        response.Length > 2
                            ? response[2]
                            : (byte)0;

                    throw new InvalidDataException(
                        string.Format(
                            "设备返回 Modbus 异常码：0x{0:X2}",
                            exceptionCode));
                }

                Log(
                    "修改命令发送完成。");

                device.CurrentAddress =
                    device.NewAddress;

                device.MarkStatus(
                    string.Format(
                        "修改命令已发送：{0} → {1}",
                        device.CurrentAddress,
                        device.NewAddress));

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

            ReadDevice(
                _devices[
                    _selectedIndex]);

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

                VerifyNewAddress(
                    device);
            }

            Log(
                "========== 修改结束 ==========");
        }

        private void ReadAllDevices(
            object sender,
            EventArgs e)
        {
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
                        5,
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
                    .Cells["ReadFunction"]
                    .Value =
                    device.ReadFunctionCode
                        .ToString("X2");

                _grid.Rows[row]
                    .Cells["WriteFunction"]
                    .Value =
                    device.WriteFunctionCode
                        .ToString("X2");

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
                    .Cells["Custom"]
                    .Value =
                    device.UseCustomWriteFrame
                        ? "是"
                        : "否";

                _grid.Rows[row]
                    .Cells["Status"]
                    .Value =
                    device.Status;
            }
        }

        private void Grid_SelectionChanged(
            object sender,
            EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0)
                return;

            int index =
                _grid.SelectedRows[0].Index;

            if (index < 0 ||
                index >= _devices.Count)
                return;

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

            _grid.ClearSelection();

            _grid.Rows[index]
                .Selected = true;

            _selectedIndex =
                index;

            LoadDevice(
                _devices[index]);
        }

        private void LoadDevice(
            DeviceProfile device)
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
        }

        // ============================================================
        // 导入导出
        // ============================================================

        private void ExportDevices(
            object sender,
            EventArgs e)
        {
            SaveEditor();

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
