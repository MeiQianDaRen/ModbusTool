using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
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

        private Button _btnConnect;
        private Button _btnDisconnect;
        private Button _btnRefresh;

        private ComboBox _cmbReadFunction;
        private ComboBox _cmbWriteFunction;

        private TextBox _txtName;
        private TextBox _txtRegister;
        private TextBox _txtCurrentAddress;
        private TextBox _txtNewAddress;

        private NumericUpDown _numReadFunction;
        private NumericUpDown _numWriteFunction;

        private RadioButton _radioInteger;
        private RadioButton _radioHex;
        private RadioButton _radioBinary;

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

            InitializeSerialControls();

            InitializeFunctionControls();

            InitializeGrid();

            InitializeDevices();

            RefreshPorts();

            SetConnectedState(false);

            Log("Modbus RTU 地址修改工具启动。");
            Log("默认通讯：9600 / None / 8 / 1");
            Log("读取功能码：03");
            Log("写入功能码：06");
        }

        // ============================================================
        // 窗体
        // ============================================================

        private void InitializeForm()
        {
            Text = "Modbus RTU RS485 地址修改工具";

            Width = 1280;

            Height = 820;

            StartPosition =
                FormStartPosition.CenterScreen;

            MinimumSize =
                new System.Drawing.Size(1000, 650);

            FormClosing += MainForm_FormClosing;
        }

        private void InitializeSerialControls()
        {
            var top = new TableLayoutPanel();

            top.Dock = DockStyle.Top;

            top.Height = 110;

            top.Padding =
                new Padding(8);

            top.ColumnCount = 12;

            top.RowCount = 2;

            for (int i = 0; i < 12; i++)
            {
                top.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        8.333f));
            }

            Controls.Add(top);

            top.Controls.Add(
                new Label
                {
                    Text = "串口",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                0,
                0);

            _cmbPort = new ComboBox();

            _cmbPort.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cmbPort.Dock = DockStyle.Fill;

            top.Controls.Add(
                _cmbPort,
                1,
                0);

            _btnRefresh =
                new Button
                {
                    Text = "刷新串口",
                    Dock = DockStyle.Fill
                };

            _btnRefresh.Click +=
                delegate
                {
                    RefreshPorts();
                };

            top.Controls.Add(
                _btnRefresh,
                2,
                0);

            top.Controls.Add(
                new Label
                {
                    Text = "波特率",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                3,
                0);

            _cmbBaud = new ComboBox();

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

            _cmbBaud.SelectedItem = "9600";

            _cmbBaud.Dock = DockStyle.Fill;

            top.Controls.Add(
                _cmbBaud,
                4,
                0);

            top.Controls.Add(
                new Label
                {
                    Text = "校验位",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                5,
                0);

            _cmbParity = new ComboBox();

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

            _cmbParity.SelectedIndex = 0;

            _cmbParity.Dock = DockStyle.Fill;

            top.Controls.Add(
                _cmbParity,
                6,
                0);

            top.Controls.Add(
                new Label
                {
                    Text = "数据位",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                7,
                0);

            _cmbDataBits = new ComboBox();

            _cmbDataBits.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cmbDataBits.Items.AddRange(
                new object[]
                {
                    "8",
                    "7"
                });

            _cmbDataBits.SelectedItem = "8";

            _cmbDataBits.Dock = DockStyle.Fill;

            top.Controls.Add(
                _cmbDataBits,
                8,
                0);

            top.Controls.Add(
                new Label
                {
                    Text = "停止位",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                9,
                0);

            _cmbStopBits = new ComboBox();

            _cmbStopBits.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cmbStopBits.Items.AddRange(
                new object[]
                {
                    "1",
                    "1.5",
                    "2"
                });

            _cmbStopBits.SelectedItem = "1";

            _cmbStopBits.Dock = DockStyle.Fill;

            top.Controls.Add(
                _cmbStopBits,
                10,
                0);

            var serialButtons =
                new FlowLayoutPanel();

            serialButtons.Dock =
                DockStyle.Fill;

            _btnConnect =
                new Button
                {
                    Text = "连接",
                    Width = 75
                };

            _btnDisconnect =
                new Button
                {
                    Text = "断开",
                    Width = 75
                };

            _btnConnect.Click +=
                delegate
                {
                    Connect();
                };

            _btnDisconnect.Click +=
                delegate
                {
                    Disconnect();
                };

            serialButtons.Controls.Add(
                _btnConnect);

            serialButtons.Controls.Add(
                _btnDisconnect);

            top.Controls.Add(
                serialButtons,
                11,
                0);

            // 第二行
            top.Controls.Add(
                new Label
                {
                    Text = "读取功能码",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                0,
                1);

            _cmbReadFunction =
                new ComboBox();

            _cmbReadFunction.DropDownStyle =
                ComboBoxStyle.DropDown;

            _cmbReadFunction.Items.AddRange(
                new object[]
                {
                    "01",
                    "02",
                    "03",
                    "04",
                    "41",
                    "42"
                });

            _cmbReadFunction.Text = "03";

            _cmbReadFunction.Dock = DockStyle.Fill;

            top.Controls.Add(
                _cmbReadFunction,
                1,
                1);

            top.Controls.Add(
                new Label
                {
                    Text = "写入功能码",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleCenter
                },
                2,
                1);

            _cmbWriteFunction =
                new ComboBox();

            _cmbWriteFunction.DropDownStyle =
                ComboBoxStyle.DropDown;

            _cmbWriteFunction.Items.AddRange(
                new object[]
                {
                    "05",
                    "06",
                    "0F",
                    "10",
                    "41",
                    "42"
                });

            _cmbWriteFunction.Text = "06";

            _cmbWriteFunction.Dock = DockStyle.Fill;

            top.Controls.Add(
                _cmbWriteFunction,
                3,
                1);

            top.Controls.Add(
                new Label
                {
                    Text = "说明：功能码可直接输入厂家自定义值",
                    Dock = DockStyle.Fill,
                    TextAlign =
                        System.Drawing.ContentAlignment.MiddleLeft
                },
                4,
                1);

            top.SetColumnSpan(
                top.Controls[ top.Controls.Count - 1 ],
                4);
        }

        private void InitializeFunctionControls()
        {
            var panel =
                new FlowLayoutPanel();

            panel.Dock = DockStyle.Top;

            panel.Height = 45;

            panel.Padding =
                new Padding(8);

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

            panel.Controls.Add(
                new Label
                {
                    Text = "数据显示：",
                    AutoSize = true,
                    Padding = new Padding(0, 6, 10, 0)
                });

            panel.Controls.Add(
                _radioInteger);

            panel.Controls.Add(
                _radioHex);

            panel.Controls.Add(
                _radioBinary);

            Controls.Add(panel);
        }

        // ============================================================
        // 设备编辑区域
        // ============================================================

        private void InitializeGrid()
        {
            _grid = new DataGridView();

            _grid.Dock =
                DockStyle.Fill;

            _grid.AllowUserToAddRows =
                false;

            _grid.AllowUserToDeleteRows =
                false;

            _grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _grid.MultiSelect = false;

            _grid.AutoGenerateColumns = false;

            AddTextColumn(
                "Name",
                "名称",
                150);

            AddTextColumn(
                "ReadFunction",
                "读功能码",
                80);

            AddTextColumn(
                "WriteFunction",
                "写功能码",
                80);

            AddTextColumn(
                "Register",
                "寄存器地址",
                110);

            AddTextColumn(
                "Current",
                "当前地址",
                100);

            AddTextColumn(
                "New",
                "修改地址",
                100);

            AddTextColumn(
                "Status",
                "状态",
                200);

            AddTextColumn(
                "Time",
                "时间",
                150);

            _grid.SelectionChanged +=
                Grid_SelectionChanged;

            var bottom =
                new Panel();

            bottom.Dock =
                DockStyle.Bottom;

            bottom.Height = 110;

            var buttons =
                new FlowLayoutPanel();

            buttons.Dock =
                DockStyle.Top;

            buttons.Height = 50;

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

            bottom.Controls.Add(buttons);

            var editor =
                new FlowLayoutPanel();

            editor.Dock =
                DockStyle.Bottom;

            editor.Height = 55;

            editor.Padding =
                new Padding(8);

            editor.WrapContents = false;

            editor.Controls.Add(
                new Label
                {
                    Text = "名称",
                    Width = 40,
                    Padding = new Padding(0, 6, 0, 0)
                });

            _txtName =
                CreateTextBox(120);

            editor.Controls.Add(_txtName);

            editor.Controls.Add(
                new Label
                {
                    Text = "寄存器",
                    Width = 55,
                    Padding = new Padding(0, 6, 0, 0)
                });

            _txtRegister =
                CreateTextBox(90);

            editor.Controls.Add(_txtRegister);

            editor.Controls.Add(
                new Label
                {
                    Text = "当前地址",
                    Width = 65,
                    Padding = new Padding(0, 6, 0, 0)
                });

            _txtCurrentAddress =
                CreateTextBox(70);

            editor.Controls.Add(
                _txtCurrentAddress);

            editor.Controls.Add(
                new Label
                {
                    Text = "修改地址",
                    Width = 65,
                    Padding = new Padding(0, 6, 0, 0)
                });

            _txtNewAddress =
                CreateTextBox(70);

            editor.Controls.Add(
                _txtNewAddress);

            editor.Controls.Add(
                new Label
                {
                    Text = "说明：寄存器地址支持 00D0、0x00D0 或十进制",
                    AutoSize = true,
                    Padding = new Padding(10, 6, 0, 0)
                });

            bottom.Controls.Add(editor);

            Controls.Add(_grid);

            Controls.Add(bottom);
        }

        private void AddTextColumn(
            string name,
            string header,
            int width)
        {
            var column =
                new DataGridViewTextBoxColumn();

            column.Name = name;

            column.HeaderText = header;

            column.Width = width;

            column.ReadOnly = true;

            _grid.Columns.Add(column);
        }

        private Button CreateButton(
            string text,
            EventHandler handler)
        {
            var button =
                new Button
                {
                    Text = text,
                    Width = 100,
                    Height = 32
                };

            button.Click += handler;

            return button;
        }

        private TextBox CreateTextBox(int width)
        {
            return new TextBox
            {
                Width = width,
                Height = 28
            };
        }

        // ============================================================
        // 初始化设备
        // ============================================================

        private void InitializeDevices()
        {
            if (_devices.Count == 0)
            {
                _devices.Add(
                    new DeviceProfile
                    {
                        Name = "设备1",
                        ReadFunctionCode = 3,
                        WriteFunctionCode = 6,
                        RegisterAddress = 0x00D0,
                        CurrentAddress = 1,
                        NewAddress = 2
                    });
            }

            RefreshGrid();

            SelectDevice(0);
        }

        // ============================================================
        // 串口
        // ============================================================

        private void RefreshPorts()
        {
            string old =
                _cmbPort.SelectedItem as string;

            _cmbPort.Items.Clear();

            string[] ports =
                SerialPort.GetPortNames();

            Array.Sort(ports);

            foreach (string port in ports)
            {
                _cmbPort.Items.Add(port);
            }

            if (!string.IsNullOrEmpty(old) &&
                _cmbPort.Items.Contains(old))
            {
                _cmbPort.SelectedItem = old;
            }
            else if (_cmbPort.Items.Count > 0)
            {
                _cmbPort.SelectedIndex = 0;
            }

            Log(
                "串口列表：" +
                (ports.Length == 0
                    ? "没有发现串口"
                    : string.Join(", ", ports)));
        }

        private void Connect()
        {
            try
            {
                if (_cmbPort.SelectedItem == null)
                {
                    MessageBox.Show(
                        "请选择串口。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                Disconnect();

                string port =
                    _cmbPort.SelectedItem.ToString();

                int baud =
                    int.Parse(
                        _cmbBaud.SelectedItem.ToString());

                Parity parity =
                    ParseParity(
                        _cmbParity.SelectedItem.ToString());

                int dataBits =
                    int.Parse(
                        _cmbDataBits.SelectedItem.ToString());

                StopBits stopBits =
                    ParseStopBits(
                        _cmbStopBits.SelectedItem.ToString());

                _serialPort =
                    new SerialPort(
                        port,
                        baud,
                        parity,
                        dataBits,
                        stopBits);

                _serialPort.ReadTimeout = 1200;

                _serialPort.WriteTimeout = 1200;

                _serialPort.ReadBufferSize = 4096;

                _serialPort.WriteBufferSize = 4096;

                _serialPort.Open();

                SetConnectedState(true);

                Log(
                    string.Format(
                        "连接成功：{0} / {1} / {2} / {3}Bits / {4}",
                        port,
                        baud,
                        parity,
                        dataBits,
                        stopBits));
            }
            catch (Exception ex)
            {
                Log("连接失败：" + ex.Message);

                MessageBox.Show(
                    "连接失败：\r\n" + ex.Message,
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Disconnect();
            }
        }

        private void Disconnect()
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

            _serialPort = null;

            SetConnectedState(false);
        }

        private void SetConnectedState(
            bool connected)
        {
            _btnConnect.Enabled =
                !connected;

            _btnDisconnect.Enabled =
                connected;

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

            _btnRefresh.Enabled =
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
            string value)
        {
            switch (value)
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
            string value)
        {
            switch (value)
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
        // 地址解析
        // ============================================================

        private static bool TryParseRegister(
            string text,
            out ushort value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();

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
                    (c >= 'A' && c <= 'F') ||
                    (c >= 'a' && c <= 'f')))
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

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();

            int number;

            bool ok;

            if (text.StartsWith(
                "0x",
                StringComparison.OrdinalIgnoreCase))
            {
                ok =
                    int.TryParse(
                        text.Substring(2),
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture,
                        out number);
            }
            else if (
                text.Any(
                    c =>
                        (c >= 'A' && c <= 'F') ||
                        (c >= 'a' && c <= 'f')))
            {
                ok =
                    int.TryParse(
                        text,
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture,
                        out number);
            }
            else
            {
                ok =
                    int.TryParse(
                        text,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out number);
            }

            if (!ok)
            {
                return false;
            }

            if (number < 1 || number > 247)
            {
                return false;
            }

            value = (byte)number;

            return true;
        }

        private static bool TryParseFunctionCode(
            string text,
            out byte value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();

            int number;

            bool ok;

            if (text.StartsWith(
                "0x",
                StringComparison.OrdinalIgnoreCase))
            {
                ok =
                    int.TryParse(
                        text.Substring(2),
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture,
                        out number);
            }
            else
            {
                ok =
                    int.TryParse(
                        text,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out number);
            }

            if (!ok ||
                number < 1 ||
                number > 255)
            {
                return false;
            }

            value = (byte)number;

            return true;
        }

        // ============================================================
        // CRC16
        // ============================================================

        private static ushort CalculateCrc16(
            byte[] data)
        {
            ushort crc = 0xFFFF;

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

        private static byte[] AddCrc(
            byte[] data)
        {
            ushort crc =
                CalculateCrc16(data);

            byte[] frame =
                new byte[data.Length + 2];

            Buffer.BlockCopy(
                data,
                0,
                frame,
                0,
                data.Length);

            frame[frame.Length - 2] =
                (byte)(crc & 0xFF);

            frame[frame.Length - 1] =
                (byte)(crc >> 8);

            return frame;
        }

        private static bool CheckCrc(
            byte[] frame)
        {
            if (frame == null ||
                frame.Length < 4)
            {
                return false;
            }

            byte[] data =
                new byte[frame.Length - 2];

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

            return calculated == received;
        }

        // ============================================================
        // Modbus RTU 帧
        // ============================================================

        private static byte[] BuildReadFrame(
            byte slave,
            byte function,
            ushort register,
            ushort count)
        {
            byte[] body =
            {
                slave,
                function,
                (byte)(register >> 8),
                (byte)(register & 0xFF),
                (byte)(count >> 8),
                (byte)(count & 0xFF)
            };

            return AddCrc(body);
        }

        private static byte[] BuildWriteSingleFrame(
            byte slave,
            byte function,
            ushort register,
            ushort value)
        {
            byte[] body =
            {
                slave,
                function,
                (byte)(register >> 8),
                (byte)(register & 0xFF),
                (byte)(value >> 8),
                (byte)(value & 0xFF)
            };

            return AddCrc(body);
        }

        private static byte[] BuildWriteMultipleFrame(
            byte slave,
            byte function,
            ushort register,
            ushort value)
        {
            byte[] body =
            {
                slave,
                function,
                (byte)(register >> 8),
                (byte)(register & 0xFF),

                0x00,
                0x01,

                0x02,

                (byte)(value >> 8),
                (byte)(value & 0xFF)
            };

            return AddCrc(body);
        }

        // ============================================================
        // 发送 / 接收
        // ============================================================

        private byte[] SendAndReceive(
            byte[] request,
            int minimumResponseLength)
        {
            if (_serialPort == null ||
                !_serialPort.IsOpen)
            {
                throw new InvalidOperationException(
                    "串口尚未连接。");
            }

            _serialPort.DiscardInBuffer();

            _serialPort.DiscardOutBuffer();

            Log(
                "TX: " +
                BytesToHex(request));

            _serialPort.Write(
                request,
                0,
                request.Length);

            List<byte> response =
                new List<byte>();

            DateTime deadline =
                DateTime.Now.AddMilliseconds(1500);

            while (DateTime.Now < deadline)
            {
                int available =
                    _serialPort.BytesToRead;

                if (available > 0)
                {
                    byte[] buffer =
                        new byte[available];

                    int count =
                        _serialPort.Read(
                            buffer,
                            0,
                            buffer.Length);

                    if (count > 0)
                    {
                        response.AddRange(
                            buffer);
                    }

                    if (response.Count >=
                        minimumResponseLength)
                    {
                        break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(10);
                }
            }

            if (response.Count == 0)
            {
                throw new TimeoutException(
                    "设备没有返回数据。");
            }

            byte[] result =
                response.ToArray();

            Log(
                "RX: " +
                BytesToHex(result));

            if (!CheckCrc(result))
            {
                throw new InvalidDataException(
                    "返回数据 CRC 校验失败。");
            }

            return result;
        }

        // ============================================================
        // 读取当前地址
        // ============================================================

        private bool ReadDevice(
            DeviceProfile device)
        {
            try
            {
                byte function =
                    device.ReadFunctionCode;

                ushort register =
                    device.RegisterAddress;

                byte[] request;

                if (function == 1 ||
                    function == 2 ||
                    function == 3 ||
                    function == 4)
                {
                    request =
                        BuildReadFrame(
                            device.CurrentAddress,
                            function,
                            register,
                            device.RegisterCount);
                }
                else
                {
                    /*
                     * 厂家自定义功能码：
                     *
                     * 默认仍采用：
                     *
                     * 地址
                     * 功能码
                     * 寄存器高
                     * 寄存器低
                     * 数量高
                     * 数量低
                     * CRC
                     *
                     * 如果厂家协议不同，
                     * 后续可以在这里扩展。
                     */

                    request =
                        BuildReadFrame(
                            device.CurrentAddress,
                            function,
                            register,
                            device.RegisterCount);
                }

                byte[] response =
                    SendAndReceive(
                        request,
                        5);

                if (response.Length < 5)
                {
                    throw new InvalidDataException(
                        "返回数据长度不足。");
                }

                if (response[0] !=
                    device.CurrentAddress)
                {
                    throw new InvalidDataException(
                        "返回设备地址与请求地址不一致。");
                }

                if (response[1] != function)
                {
                    if ((response[1] & 0x80) != 0)
                    {
                        byte exceptionCode =
                            response.Length > 2
                                ? response[2]
                                : (byte)0;

                        throw new InvalidDataException(
                            string.Format(
                                "Modbus 异常响应，异常码：0x{0:X2}",
                                exceptionCode));
                    }

                    throw new InvalidDataException(
                        "返回功能码不一致。");
                }

                int byteCount =
                    response[2];

                if (response.Length <
                    3 + byteCount + 2)
                {
                    throw new InvalidDataException(
                        "返回数据长度错误。");
                }

                if (byteCount < 2)
                {
                    throw new InvalidDataException(
                        "返回寄存器数据不足。");
                }

                ushort value =
                    (ushort)(
                        (response[3] << 8) |
                        response[4]);

                if (value < 1 ||
                    value > 247)
                {
                    throw new InvalidDataException(
                        "读取到的地址值不在 1～247 范围内：" +
                        value);
                }

                device.CurrentAddress =
                    (byte)value;

                device.MarkStatus(
                    "读取成功：" +
                    FormatAddress(
                        device.CurrentAddress));

                return true;
            }
            catch (Exception ex)
            {
                device.MarkStatus(
                    "读取失败：" +
                    ex.Message);

                Log(
                    "读取失败：" +
                    ex.Message);

                return false;
            }
        }

        // ============================================================
        // 修改地址
        // ============================================================

        private bool WriteDevice(
            DeviceProfile device)
        {
            try
            {
                byte oldAddress =
                    device.CurrentAddress;

                byte newAddress =
                    device.NewAddress;

                if (oldAddress < 1 ||
                    oldAddress > 247)
                {
                    throw new ArgumentException(
                        "当前地址必须在 1～247。");
                }

                if (newAddress < 1 ||
                    newAddress > 247)
                {
                    throw new ArgumentException(
                        "修改地址必须在 1～247。");
                }

                if (oldAddress == newAddress)
                {
                    throw new ArgumentException(
                        "当前地址与修改地址不能相同。");
                }

                byte function =
                    device.WriteFunctionCode;

                byte[] request;

                if (function == 6)
                {
                    request =
                        BuildWriteSingleFrame(
                            oldAddress,
                            function,
                            device.RegisterAddress,
                            newAddress);
                }
                else if (function == 16)
                {
                    request =
                        BuildWriteMultipleFrame(
                            oldAddress,
                            function,
                            device.RegisterAddress,
                            newAddress);
                }
                else
                {
                    /*
                     * 厂家自定义写入功能码：
                     *
                     * 默认采用类似 06 的结构：
                     *
                     * 地址
                     * 功能码
                     * 寄存器高
                     * 寄存器低
                     * 新地址高
                     * 新地址低
                     * CRC
                     *
                     * 如果厂家协议完全不同，
                     * 可以继续扩展这里。
                     */

                    request =
                        BuildWriteSingleFrame(
                            oldAddress,
                            function,
                            device.RegisterAddress,
                            newAddress);
                }

                byte[] response =
                    SendAndReceive(
                        request,
                        8);

                /*
                 * 标准 06：
                 *
                 * 回复应该与请求数据区一致。
                 */

                if (response.Length >= 6)
                {
                    if (response[0] != oldAddress)
                    {
                        throw new InvalidDataException(
                            "写入响应地址不一致。");
                    }

                    if (response[1] != function)
                    {
                        if ((response[1] & 0x80) != 0)
                        {
                            byte exceptionCode =
                                response.Length > 2
                                    ? response[2]
                                    : (byte)0;

                            throw new InvalidDataException(
                                string.Format(
                                    "设备拒绝修改，异常码：0x{0:X2}",
                                    exceptionCode));
                        }

                        throw new InvalidDataException(
                            "写入响应功能码不一致。");
                    }
                }

                /*
                 * 很多设备改地址后会立即切换到新地址。
                 *
                 * 因此这里把当前地址先改成新地址。
                 */

                device.CurrentAddress =
                    newAddress;

                device.MarkStatus(
                    string.Format(
                        "修改成功：{0} → {1}",
                        FormatAddress(oldAddress),
                        FormatAddress(newAddress)));

                Log(
                    string.Format(
                        "地址修改成功：{0} → {1}",
                        FormatAddress(oldAddress),
                        FormatAddress(newAddress)));

                return true;
            }
            catch (Exception ex)
            {
                device.MarkStatus(
                    "修改失败：" +
                    ex.Message);

                Log(
                    "修改失败：" +
                    ex.Message);

                return false;
            }
        }

        // ============================================================
        // 修改后验证
        // ============================================================

        private bool VerifyNewAddress(
            DeviceProfile device)
        {
            try
            {
                /*
                 * 修改完成后：
                 *
                 * 设备地址应该已经从旧地址
                 * 变成新地址。
                 *
                 * 因此使用新地址再次读取。
                 */

                byte newAddress =
                    device.NewAddress;

                byte function =
                    device.ReadFunctionCode;

                byte[] request =
                    BuildReadFrame(
                        newAddress,
                        function,
                        device.RegisterAddress,
                        device.RegisterCount);

                byte[] response =
                    SendAndReceive(
                        request,
                        5);

                if (response[0] != newAddress)
                {
                    throw new InvalidDataException(
                        "验证返回地址不正确。");
                }

                if (response[1] != function)
                {
                    throw new InvalidDataException(
                        "验证返回功能码不正确。");
                }

                int byteCount =
                    response[2];

                if (byteCount < 2)
                {
                    throw new InvalidDataException(
                        "验证数据不足。");
                }

                ushort value =
                    (ushort)(
                        (response[3] << 8) |
                        response[4]);

                if (value != newAddress)
                {
                    throw new InvalidDataException(
                        string.Format(
                            "验证失败：寄存器返回 {0}，期望 {1}",
                            value,
                            newAddress));
                }

                device.CurrentAddress =
                    newAddress;

                device.MarkStatus(
                    "修改并验证成功：" +
                    FormatAddress(newAddress));

                Log(
                    "修改后验证成功。");

                return true;
            }
            catch (Exception ex)
            {
                device.MarkStatus(
                    "修改成功，但验证失败：" +
                    ex.Message);

                Log(
                    "修改后验证失败：" +
                    ex.Message);

                return false;
            }
        }

        // ============================================================
        // 按钮操作
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
        }

        private void DeleteDevice(
            object sender,
            EventArgs e)
        {
            if (_selectedIndex < 0 ||
                _selectedIndex >= _devices.Count)
            {
                return;
            }

            if (MessageBox.Show(
                "确定删除当前设备？",
                "确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
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

        private void ReadSelectedDevice(
            object sender,
            EventArgs e)
        {
            if (!SaveEditorToSelected())
            {
                return;
            }

            if (_selectedIndex < 0)
            {
                return;
            }

            DeviceProfile device =
                _devices[_selectedIndex];

            ReadDevice(device);

            RefreshGrid();

            SelectDevice(_selectedIndex);
        }

        private void WriteSelectedDevice(
            object sender,
            EventArgs e)
        {
            if (!SaveEditorToSelected())
            {
                return;
            }

            if (_selectedIndex < 0)
            {
                return;
            }

            DeviceProfile device =
                _devices[_selectedIndex];

            if (MessageBox.Show(
                string.Format(
                    "确定将地址 {0} 修改为 {1}？",
                    device.CurrentAddress,
                    device.NewAddress),
                "确认修改",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)
                != DialogResult.Yes)
            {
                return;
            }

            bool ok =
                WriteDevice(device);

            if (ok)
            {
                /*
                 * 修改后自动验证。
                 */

                VerifyNewAddress(device);
            }

            RefreshGrid();

            SelectDevice(_selectedIndex);
        }

        private void ReadAllDevices(
            object sender,
            EventArgs e)
        {
            for (int i = 0;
                 i < _devices.Count;
                 i++)
            {
                if (!SaveEditorToDevice(
                    _devices[i]))
                {
                    continue;
                }

                ReadDevice(
                    _devices[i]);

                RefreshGrid();

                Application.DoEvents();
            }
        }

        private void WriteAllDevices(
            object sender,
            EventArgs e)
        {
            if (MessageBox.Show(
                "确定按照列表中的当前地址和修改地址批量修改？",
                "确认批量修改",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)
                != DialogResult.Yes)
            {
                return;
            }

            for (int i = 0;
                 i < _devices.Count;
                 i++)
            {
                DeviceProfile device =
                    _devices[i];

                if (!SaveEditorToDevice(device))
                {
                    continue;
                }

                if (WriteDevice(device))
                {
                    VerifyNewAddress(device);
                }

                RefreshGrid();

                Application.DoEvents();
            }
        }

        // ============================================================
        // 导入 / 导出
        // ============================================================

        private void ExportDevices(
            object sender,
            EventArgs e)
        {
            SaveEditorToSelected();

            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Filter =
                    "JSON 文件 (*.json)|*.json";

                dialog.FileName =
                    "ModbusAddressDevices.json";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    var serializer =
                        new DataContractJsonSerializer(
                            typeof(List<DeviceProfile>));

                    using (FileStream stream =
                        File.Create(dialog.FileName))
                    {
                        serializer.WriteObject(
                            stream,
                            _devices);
                    }

                    Log(
                        "导出成功：" +
                        dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "导出失败：\r\n" +
                        ex.Message);
                }
            }
        }

        private void ImportDevices(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog dialog =
                new OpenFileDialog())
            {
                dialog.Filter =
                    "JSON 文件 (*.json)|*.json";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    var serializer =
                        new DataContractJsonSerializer(
                            typeof(List<DeviceProfile>));

                    List<DeviceProfile> result;

                    using (FileStream stream =
                        File.OpenRead(dialog.FileName))
                    {
                        result =
                            serializer.ReadObject(stream)
                            as List<DeviceProfile>;
                    }

                    if (result == null)
                    {
                        throw new InvalidDataException(
                            "JSON 数据格式错误。");
                    }

                    _devices.Clear();

                    _devices.AddRange(result);

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
                        "导入成功：" +
                        dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "导入失败：\r\n" +
                        ex.Message,
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // ============================================================
        // Grid
        // ============================================================

        private void RefreshGrid()
        {
            if (_grid == null)
            {
                return;
            }

            _grid.Rows.Clear();

            foreach (DeviceProfile device
                in _devices)
            {
                int row =
                    _grid.Rows.Add();

                _grid.Rows[row].Cells["Name"].Value =
                    device.Name;

                _grid.Rows[row].Cells["ReadFunction"].Value =
                    device.ReadFunctionCode.ToString("X2");

                _grid.Rows[row].Cells["WriteFunction"].Value =
                    device.WriteFunctionCode.ToString("X2");

                _grid.Rows[row].Cells["Register"].Value =
                    FormatRegister(
                        device.RegisterAddress);

                _grid.Rows[row].Cells["Current"].Value =
                    FormatAddress(
                        device.CurrentAddress);

                _grid.Rows[row].Cells["New"].Value =
                    FormatAddress(
                        device.NewAddress);

                _grid.Rows[row].Cells["Status"].Value =
                    device.Status;

                _grid.Rows[row].Cells["Time"].Value =
                    device.LastOperationTime;
            }
        }

        private void Grid_SelectionChanged(
            object sender,
            EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0)
            {
                return;
            }

            int index =
                _grid.SelectedRows[0].Index;

            if (index >= 0 &&
                index < _devices.Count)
            {
                _selectedIndex =
                    index;

                LoadDeviceToEditor(
                    _devices[index]);
            }
        }

        private void SelectDevice(
            int index)
        {
            if (_grid == null ||
                index < 0 ||
                index >= _grid.Rows.Count)
            {
                return;
            }

            _selectedIndex = index;

            _grid.ClearSelection();

            _grid.Rows[index].Selected =
                true;

            LoadDeviceToEditor(
                _devices[index]);
        }

        private void LoadDeviceToEditor(
            DeviceProfile device)
        {
            _txtName.Text =
                device.Name;

            _txtRegister.Text =
                FormatRegister(
                    device.RegisterAddress);

            _txtCurrentAddress.Text =
                FormatAddress(
                    device.CurrentAddress);

            _txtNewAddress.Text =
                FormatAddress(
                    device.NewAddress);

            _cmbReadFunction.Text =
                device.ReadFunctionCode.ToString("X2");

            _cmbWriteFunction.Text =
                device.WriteFunctionCode.ToString("X2");
        }

        private bool SaveEditorToSelected()
        {
            if (_selectedIndex < 0 ||
                _selectedIndex >= _devices.Count)
            {
                return false;
            }

            return SaveEditorToDevice(
                _devices[_selectedIndex]);
        }

        private bool SaveEditorToDevice(
            DeviceProfile device)
        {
            byte readFunction;

            byte writeFunction;

            ushort register;

            byte currentAddress;

            byte newAddress;

            if (!TryParseFunctionCode(
                _cmbReadFunction.Text,
                out readFunction))
            {
                MessageBox.Show(
                    "读取功能码无效。",
                    "输入错误");

                return false;
            }

            if (!TryParseFunctionCode(
                _cmbWriteFunction.Text,
                out writeFunction))
            {
                MessageBox.Show(
                    "写入功能码无效。",
                    "输入错误");

                return false;
            }

            if (!TryParseRegister(
                _txtRegister.Text,
                out register))
            {
                MessageBox.Show(
                    "寄存器地址无效。\r\n\r\n例如：00D0、0x00D0、208",
                    "输入错误");

                return false;
            }

            if (!TryParseAddress(
                _txtCurrentAddress.Text,
                out currentAddress))
            {
                MessageBox.Show(
                    "当前地址必须是 1～247。",
                    "输入错误");

                return false;
            }

            if (!TryParseAddress(
                _txtNewAddress.Text,
                out newAddress))
            {
                MessageBox.Show(
                    "修改地址必须是 1～247。",
                    "输入错误");

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
                currentAddress;

            device.NewAddress =
                newAddress;

            return true;
        }

        // ============================================================
        // 显示格式
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

        private string FormatAddress(
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
                        2).PadLeft(
                            8,
                            '0');

                default:

                    return value.ToString();
            }
        }

        private string FormatRegister(
            ushort value)
        {
            return value.ToString("X4");
        }

        // ============================================================
        // 日志
        // ============================================================

        private void Log(
            string message)
        {
            if (_txtLog == null)
            {
                EnsureLogControl();
            }

            if (_txtLog == null)
            {
                return;
            }

            _txtLog.AppendText(
                string.Format(
                    "[{0}] {1}\r\n",
                    DateTime.Now.ToString(
                        "HH:mm:ss.fff"),
                    message));

            _txtLog.SelectionStart =
                _txtLog.TextLength;

            _txtLog.ScrollToCaret();
        }

        private void EnsureLogControl()
        {
            if (_txtLog != null)
            {
                return;
            }

            _txtLog =
                new RichTextBox();

            _txtLog.Dock =
                DockStyle.Bottom;

            _txtLog.Height =
                150;

            _txtLog.ReadOnly =
                true;

            _txtLog.Font =
                new System.Drawing.Font(
                    "Consolas",
                    9);

            Controls.Add(_txtLog);
        }

        private static string BytesToHex(
            byte[] bytes)
        {
            if (bytes == null)
            {
                return "";
            }

            return string.Join(
                " ",
                bytes.Select(
                    b => b.ToString("X2")));
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
