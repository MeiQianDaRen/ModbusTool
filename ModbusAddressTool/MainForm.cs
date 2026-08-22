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
    public partial class MainForm : Form
    {
        private SerialPort _serialPort;

        private readonly List<DeviceProfile> _devices =
            new List<DeviceProfile>();

        private int _selectedDeviceIndex = -1;

        private enum DisplayFormat
        {
            Integer,
            Hex,
            Binary
        }

        private DisplayFormat _displayFormat = DisplayFormat.Integer;

        public MainForm()
        {
            InitializeComponent();
        }

        #region Form

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeSerialSettings();
            InitializeFunctionCodes();
            InitializeDeviceGrid();
            RefreshPorts();

            SetConnectedState(false);

            Log("Modbus RS485 地址修改工具启动。");
            Log("默认通讯参数：9600 / None / 8 / 1");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        #endregion

        #region 初始化

        private void InitializeSerialSettings()
        {
            cboBaud.Items.Clear();

            int[] baudRates =
            {
                1200,
                2400,
                4800,
                9600,
                19200,
                38400,
                57600,
                115200
            };

            foreach (int baud in baudRates)
            {
                cboBaud.Items.Add(baud.ToString());
            }

            cboBaud.SelectedItem = "9600";

            cboParity.Items.Clear();
            cboParity.Items.Add("None");
            cboParity.Items.Add("Even");
            cboParity.Items.Add("Odd");
            cboParity.Items.Add("Mark");
            cboParity.Items.Add("Space");
            cboParity.SelectedIndex = 0;

            cboDataBits.Items.Clear();
            cboDataBits.Items.Add("8");
            cboDataBits.Items.Add("7");
            cboDataBits.SelectedItem = "8";

            cboStopBits.Items.Clear();
            cboStopBits.Items.Add("1");
            cboStopBits.Items.Add("1.5");
            cboStopBits.Items.Add("2");
            cboStopBits.SelectedItem = "1";
        }

        private void InitializeFunctionCodes()
        {
            cboFunction.Items.Clear();

            cboFunction.Items.Add("06");
            cboFunction.Items.Add("10");

            cboFunction.SelectedIndex = 0;
        }

        private void InitializeDeviceGrid()
        {
            dgvDevices.Columns.Clear();

            dgvDevices.Columns.Add("Name", "名称");
            dgvDevices.Columns.Add("FunctionCode", "功能码");
            dgvDevices.Columns.Add("Register", "寄存器");
            dgvDevices.Columns.Add("CurrentAddress", "当前地址");
            dgvDevices.Columns.Add("NewAddress", "修改地址");
            dgvDevices.Columns.Add("Status", "状态");
            dgvDevices.Columns.Add("Time", "时间");

            dgvDevices.Columns["Name"].FillWeight = 130;
            dgvDevices.Columns["FunctionCode"].FillWeight = 60;
            dgvDevices.Columns["Register"].FillWeight = 80;
            dgvDevices.Columns["CurrentAddress"].FillWeight = 80;
            dgvDevices.Columns["NewAddress"].FillWeight = 80;
            dgvDevices.Columns["Status"].FillWeight = 150;
            dgvDevices.Columns["Time"].FillWeight = 140;

            RefreshDeviceGrid();
        }

        #endregion

        #region 串口

        private void RefreshPorts()
        {
            string selected = cboPort.SelectedItem as string;

            cboPort.Items.Clear();

            string[] ports = SerialPort.GetPortNames();

            Array.Sort(ports);

            foreach (string port in ports)
            {
                cboPort.Items.Add(port);
            }

            if (!string.IsNullOrEmpty(selected) &&
                cboPort.Items.Contains(selected))
            {
                cboPort.SelectedItem = selected;
            }
            else if (cboPort.Items.Count > 0)
            {
                cboPort.SelectedIndex = 0;
            }
        }

        private void btnRefreshPort_Click(object sender, EventArgs e)
        {
            RefreshPorts();
            Log("已刷新串口列表。");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Connect()
        {
            try
            {
                if (cboPort.SelectedItem == null)
                {
                    MessageBox.Show(
                        "请选择串口。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (_serialPort != null && _serialPort.IsOpen)
                {
                    return;
                }

                string portName = cboPort.SelectedItem.ToString();

                int baudRate =
                    int.Parse(cboBaud.SelectedItem.ToString());

                Parity parity =
                    ParseParity(cboParity.SelectedItem.ToString());

                int dataBits =
                    int.Parse(cboDataBits.SelectedItem.ToString());

                StopBits stopBits =
                    ParseStopBits(cboStopBits.SelectedItem.ToString());

                _serialPort = new SerialPort(
                    portName,
                    baudRate,
                    parity,
                    dataBits,
                    stopBits);

                _serialPort.ReadTimeout = 1000;
                _serialPort.WriteTimeout = 1000;

                _serialPort.Open();

                SetConnectedState(true);

                Log(
                    string.Format(
                        "已连接：{0}, {1}, {2}, {3}Bits, {4}StopBits",
                        portName,
                        baudRate,
                        parity,
                        dataBits,
                        stopBits));
            }
            catch (Exception ex)
            {
                Log("连接失败：" + ex.Message);

                if (_serialPort != null)
                {
                    try
                    {
                        _serialPort.Dispose();
                    }
                    catch
                    {
                    }

                    _serialPort = null;
                }

                MessageBox.Show(
                    "连接失败：\r\n" + ex.Message,
                    "连接错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                    _serialPort = null;
                }
            }
            catch
            {
            }

            SetConnectedState(false);
        }

        private void SetConnectedState(bool connected)
        {
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;

            cboPort.Enabled = !connected;
            cboBaud.Enabled = !connected;
            cboParity.Enabled = !connected;
            cboDataBits.Enabled = !connected;
            cboStopBits.Enabled = !connected;
            btnRefreshPort.Enabled = !connected;

            btnRead.Enabled = connected;
            btnWrite.Enabled = connected;
            btnReadAll.Enabled = connected;
            btnWriteAll.Enabled = connected;
        }

        private static Parity ParseParity(string value)
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

        private static StopBits ParseStopBits(string value)
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

        #endregion

        #region 数据格式

        private void DisplayFormatChanged(object sender, EventArgs e)
        {
            if (radioInteger.Checked)
            {
                _displayFormat = DisplayFormat.Integer;
            }
            else if (radioHex.Checked)
            {
                _displayFormat = DisplayFormat.Hex;
            }
            else
            {
                _displayFormat = DisplayFormat.Binary;
            }

            RefreshDeviceGrid();
        }

        private string FormatValue(ushort value)
        {
            switch (_displayFormat)
            {
                case DisplayFormat.Hex:
                    return "0x" + value.ToString("X4");

                case DisplayFormat.Binary:
                    return Convert.ToString(value, 2).PadLeft(16, '0');

                default:
                    return value.ToString();
            }
        }

        private string FormatAddress(byte value)
        {
            switch (_displayFormat)
            {
                case DisplayFormat.Hex:
                    return "0x" + value.ToString("X2");

                case DisplayFormat.Binary:
                    return Convert.ToString(value, 2).PadLeft(8, '0');

                default:
                    return value.ToString();
            }
        }

        #endregion

        #region 地址输入解析

        private bool TryParseRegisterAddress(
            string text,
            out ushort value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();

            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(
                    text.Substring(2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            // 含 A-F，自动按照十六进制处理
            if (text.Any(c =>
                (c >= 'A' && c <= 'F') ||
                (c >= 'a' && c <= 'f')))
            {
                return ushort.TryParse(
                    text,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            // 例如 00D3 会走上面的十六进制
            // 纯数字默认按照十进制。
            return ushort.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value);
        }

        private bool TryParseAddress(
            string text,
            out byte value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();

            int parsed;

            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(
                    text.Substring(2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out parsed))
                {
                    return false;
                }
            }
            else if (text.Any(c =>
                (c >= 'A' && c <= 'F') ||
                (c >= 'a' && c <= 'f')))
            {
                if (!int.TryParse(
                    text,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out parsed))
                {
                    return false;
                }
            }
            else
            {
                if (!int.TryParse(
                    text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out parsed))
                {
                    return false;
                }
            }

            if (parsed < 1 || parsed > 247)
            {
                return false;
            }

            value = (byte)parsed;

            return true;
        }

        #endregion

        #region Modbus RTU

        private byte[] BuildReadHoldingRegistersFrame(
            byte slaveAddress,
            ushort registerAddress,
            ushort registerCount)
        {
            byte[] frame = new byte[6];

            frame[0] = slaveAddress;
            frame[1] = 0x03;

            frame[2] = (byte)(registerAddress >> 8);
            frame[3] = (byte)(registerAddress & 0xFF);

            frame[4] = (byte)(registerCount >> 8);
            frame[5] = (byte)(registerCount & 0xFF);

            return AddCrc(frame);
        }

        private byte[] BuildWriteSingleRegisterFrame(
            byte slaveAddress,
            ushort registerAddress,
            ushort value)
        {
            byte[] frame = new byte[6];

            frame[0] = slaveAddress;
            frame[1] = 0x06;

            frame[2] = (byte)(registerAddress >> 8);
            frame[3] = (byte)(registerAddress & 0xFF);

            frame[4] = (byte)(value >> 8);
            frame[5] = (byte)(value & 0xFF);

            return AddCrc(frame);
        }

        private byte[] BuildWriteMultipleRegistersFrame(
            byte slaveAddress,
            ushort registerAddress,
            ushort[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("没有要写入的数据。");
            }

            byte[] frame = new byte[7 + values.Length * 2];

            frame[0] = slaveAddress;
            frame[1] = 0x10;

            frame[2] = (byte)(registerAddress >> 8);
            frame[3] = (byte)(registerAddress & 0xFF);

            frame[4] = (byte)(values.Length >> 8);
            frame[5] = (byte)(values.Length & 0xFF);

            frame[6] = (byte)(values.Length * 2);

            for (int i = 0; i < values.Length; i++)
            {
                frame[7 + i * 2] =
                    (byte)(values[i] >> 8);

                frame[8 + i * 2] =
                    (byte)(values[i] & 0xFF);
            }

            return AddCrc(frame);
        }

        private byte[] AddCrc(byte[] frameWithoutCrc)
        {
            ushort crc = CalculateCrc16(frameWithoutCrc);

            byte[] frame =
                new byte[frameWithoutCrc.Length + 2];

            Buffer.BlockCopy(
                frameWithoutCrc,
                0,
                frame,
                0,
                frameWithoutCrc.Length);

            // Modbus RTU CRC：低字节在前
            frame[frame.Length - 2] = (byte)(crc & 0xFF);
            frame[frame.Length - 1] = (byte)(crc >> 8);

            return frame;
        }

        private static ushort CalculateCrc16(byte[] data)
        {
            ushort crc = 0xFFFF;

            for (int pos = 0; pos < data.Length; pos++)
            {
                crc ^= data[pos];

                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
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

        private bool ValidateCrc(byte[] frame)
        {
            if (frame == null || frame.Length < 4)
            {
                return false;
            }

            ushort received =
                (ushort)(
                    frame[frame.Length - 2] |
                    (frame[frame.Length - 1] << 8));

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

            return received == calculated;
        }

        #endregion

        #region 通讯

        private byte[] SendAndReceive(
            byte[] request,
            int expectedLength,
            int timeoutMilliseconds = 1000)
        {
            if (_serialPort == null ||
                !_serialPort.IsOpen)
            {
                throw new InvalidOperationException(
                    "串口未连接。");
            }

            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();

            Log("TX: " + BytesToString(request));

            _serialPort.Write(request, 0, request.Length);

            List<byte> received =
                new List<byte>();

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds <
                   timeoutMilliseconds)
            {
                int available = _serialPort.BytesToRead;

                if (available > 0)
                {
                    byte[] buffer =
                        new byte[available];

                    int count =
                        _serialPort.Read(
                            buffer,
                            0,
                            buffer.Length);

                    for (int i = 0; i < count; i++)
                    {
                        received.Add(buffer[i]);
                    }

                    if (received.Count >= expectedLength)
                    {
                        break;
                    }
                }

                Thread.Sleep(5);
            }

            if (received.Count == 0)
            {
                throw new TimeoutException(
                    "设备没有返回数据。");
            }

            byte[] response =
                received.ToArray();

            Log("RX: " + BytesToString(response));

            if (!ValidateCrc(response))
            {
                throw new InvalidDataException(
                    "Modbus CRC 校验失败。");
            }

            return response;
        }

        private ushort ReadRegister(
            byte slaveAddress,
            ushort registerAddress)
        {
            byte[] request =
                BuildReadHoldingRegistersFrame(
                    slaveAddress,
                    registerAddress,
                    1);

            byte[] response =
                SendAndReceive(
                    request,
                    7,
                    1200);

            if (response.Length < 7)
            {
                throw new InvalidDataException(
                    "读取响应长度错误。");
            }

            if (response[0] != slaveAddress)
            {
                throw new InvalidDataException(
                    "返回设备地址不一致。");
            }

            if (response[1] == 0x83)
            {
                throw new InvalidDataException(
                    string.Format(
                        "设备返回异常码：0x{0:X2}",
                        response[2]));
            }

            if (response[1] != 0x03)
            {
                throw new InvalidDataException(
                    string.Format(
                        "返回功能码错误：0x{0:X2}",
                        response[1]));
            }

            if (response[2] != 2)
            {
                throw new InvalidDataException(
                    "读取寄存器数据长度错误。");
            }

            ushort value =
                (ushort)(
                    (response[3] << 8) |
                    response[4]);

            return value;
        }

        private void WriteRegister(
            byte slaveAddress,
            ushort registerAddress,
            byte functionCode,
            byte newAddress)
        {
            byte[] request;

            if (functionCode == 0x06)
            {
                request =
                    BuildWriteSingleRegisterFrame(
                        slaveAddress,
                        registerAddress,
                        newAddress);
            }
            else if (functionCode == 0x10)
            {
                request =
                    BuildWriteMultipleRegistersFrame(
                        slaveAddress,
                        registerAddress,
                        new ushort[]
                        {
                            newAddress
                        });
            }
            else
            {
                throw new NotSupportedException(
                    "当前仅支持功能码 06 和 10。");
            }

            int expectedLength =
                functionCode == 0x06
                    ? 8
                    : 8;

            byte[] response =
                SendAndReceive(
                    request,
                    expectedLength,
                    1200);

            if (response.Length < 8)
            {
                throw new InvalidDataException(
                    "写入响应长度错误。");
            }

            if (response[0] != slaveAddress)
            {
                throw new InvalidDataException(
                    "写入返回地址不一致。");
            }

            if (response[1] ==
                (byte)(functionCode | 0x80))
            {
                throw new InvalidDataException(
                    string.Format(
                        "设备返回异常码：0x{0:X2}",
                        response[2]));
            }

            if (response[1] != functionCode)
            {
                throw new InvalidDataException(
                    string.Format(
                        "写入返回功能码错误：0x{0:X2}",
                        response[1]));
            }
        }

        #endregion

        #region 读取

        private void btnRead_Click(object sender, EventArgs e)
        {
            try
            {
                ushort register;
                byte currentAddress;

                if (!TryReadFormValues(
                    out register,
                    out currentAddress,
                    false))
                {
                    return;
                }

                Log(
                    string.Format(
                        "开始读取：设备地址={0}，寄存器={1}",
                        FormatAddress(currentAddress),
                        FormatValue(register)));

                ushort value =
                    ReadRegister(
                        currentAddress,
                        register);

                txtCurrentAddress.Text =
                    FormatAddress((byte)value);

                Log(
                    string.Format(
                        "读取成功：寄存器 {0} = {1}",
                        FormatValue(register),
                        FormatAddress((byte)value)));

                if (value < 1 || value > 247)
                {
                    Log(
                        "警告：读取到的地址不在 Modbus 标准地址 1~247 范围内。");
                }
            }
            catch (Exception ex)
            {
                Log("读取失败：" + ex.Message);

                MessageBox.Show(
                    "读取失败：\r\n" + ex.Message,
                    "读取失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool TryReadFormValues(
            out ushort register,
            out byte currentAddress,
            bool validateNewAddress)
        {
            register = 0;
            currentAddress = 0;

            if (!TryParseRegisterAddress(
                txtRegister.Text,
                out register))
            {
                MessageBox.Show(
                    "寄存器地址格式错误。\r\n例如：00D3、211、0x00D3",
                    "输入错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtRegister.Focus();
                return false;
            }

            if (!TryParseAddress(
                txtCurrentAddress.Text,
                out currentAddress))
            {
                MessageBox.Show(
                    "当前地址必须是 1~247。",
                    "输入错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCurrentAddress.Focus();
                return false;
            }

            if (validateNewAddress)
            {
                byte newAddress;

                if (!TryParseAddress(
                    txtNewAddress.Text,
                    out newAddress))
                {
                    MessageBox.Show(
                        "修改地址必须是 1~247。",
                        "输入错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtNewAddress.Focus();
                    return false;
                }

                if (newAddress == currentAddress)
                {
                    MessageBox.Show(
                        "修改地址不能和当前地址相同。",
                        "输入错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return false;
                }
            }

            return true;
        }

        #endregion

        #region 修改地址

        private void btnWrite_Click(object sender, EventArgs e)
        {
            try
            {
                ushort register;
                byte currentAddress;

                if (!TryReadFormValues(
                    out register,
                    out currentAddress,
                    true))
                {
                    return;
                }

                byte newAddress;

                if (!TryParseAddress(
                    txtNewAddress.Text,
                    out newAddress))
                {
                    return;
                }

                if (IsNewAddressUsedByOtherDevice(
                    newAddress,
                    -1))
                {
                    DialogResult result =
                        MessageBox.Show(
                            "设备列表中已经存在相同的新地址：" +
                            FormatAddress(newAddress) +
                            "\r\n\r\n继续修改可能造成 RS485 地址冲突。\r\n是否继续？",
                            "地址冲突警告",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                DialogResult confirm =
                    MessageBox.Show(
                        string.Format(
                            "确定修改设备地址吗？\r\n\r\n当前地址：{0}\r\n修改地址：{1}\r\n寄存器：{2}\r\n功能码：{3}",
                            FormatAddress(currentAddress),
                            FormatAddress(newAddress),
                            FormatRegister(register),
                            cboFunction.SelectedItem),
                        "确认修改",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                byte functionCode =
                    byte.Parse(
                        cboFunction.SelectedItem.ToString(),
                        NumberStyles.HexNumber);

                Log(
                    string.Format(
                        "开始修改地址：{0} → {1}",
                        FormatAddress(currentAddress),
                        FormatAddress(newAddress)));

                WriteRegister(
                    currentAddress,
                    register,
                    functionCode,
                    newAddress);

                Log("写入成功。");

                // 设备地址已经改变
                Thread.Sleep(100);

                Log(
                    "开始使用新地址进行验证：" +
                    FormatAddress(newAddress));

                ushort verifyValue =
                    ReadRegister(
                        newAddress,
                        register);

                if (verifyValue != newAddress)
                {
                    throw new InvalidDataException(
                        string.Format(
                            "验证失败。设备返回地址为 {0}，期望 {1}。",
                            verifyValue,
                            newAddress));
                }

                txtCurrentAddress.Text =
                    FormatAddress(newAddress);

                Log(
                    string.Format(
                        "修改成功，验证通过：{0} → {1}",
                        FormatAddress(currentAddress),
                        FormatAddress(newAddress)));

                MessageBox.Show(
                    string.Format(
                        "地址修改成功！\r\n\r\n{0} → {1}\r\n验证通过。",
                        FormatAddress(currentAddress),
                        FormatAddress(newAddress)),
                    "成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("修改失败：" + ex.Message);

                MessageBox.Show(
                    "地址修改失败：\r\n" + ex.Message,
                    "修改失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 设备列表

        private DeviceProfile ReadDeviceFromForm()
        {
            ushort register;

            if (!TryParseRegisterAddress(
                txtRegister.Text,
                out register))
            {
                throw new ArgumentException(
                    "寄存器地址格式错误。");
            }

            byte currentAddress;

            if (!TryParseAddress(
                txtCurrentAddress.Text,
                out currentAddress))
            {
                throw new ArgumentException(
                    "当前地址必须为 1~247。");
            }

            byte newAddress;

            if (!TryParseAddress(
                txtNewAddress.Text,
                out newAddress))
            {
                throw new ArgumentException(
                    "修改地址必须为 1~247。");
            }

            byte functionCode =
                byte.Parse(
                    cboFunction.SelectedItem.ToString(),
                    NumberStyles.HexNumber);

            return new DeviceProfile
            {
                Name = txtName.Text.Trim(),
                FunctionCode = functionCode,
                RegisterAddress = register,
                CurrentAddress = currentAddress,
                NewAddress = newAddress,
                Status = "",
                LastOperationTime = ""
            };
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                DeviceProfile device =
                    ReadDeviceFromForm();

                if (string.IsNullOrWhiteSpace(device.Name))
                {
                    device.Name =
                        "设备" + (_devices.Count + 1);
                }

                _devices.Add(device);

                RefreshDeviceGrid();

                Log(
                    "已添加设备：" +
                    device.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "添加设备失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int index =
                GetSelectedDeviceIndex();

            if (index < 0)
            {
                MessageBox.Show(
                    "请选择一个设备。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            _devices.RemoveAt(index);

            _selectedDeviceIndex = -1;

            RefreshDeviceGrid();

            Log("已删除选中设备。");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (_devices.Count == 0)
            {
                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "确定清空全部设备吗？",
                    "确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _devices.Clear();

            _selectedDeviceIndex = -1;

            RefreshDeviceGrid();

            Log("已清空设备列表。");
        }

        private void dgvDevices_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.RowIndex >= _devices.Count)
            {
                return;
            }

            LoadDeviceToForm(e.RowIndex);
        }

        private int GetSelectedDeviceIndex()
        {
            if (dgvDevices.SelectedRows.Count == 0)
            {
                return -1;
            }

            return dgvDevices.SelectedRows[0].Index;
        }

        private void LoadDeviceToForm(int index)
        {
            if (index < 0 ||
                index >= _devices.Count)
            {
                return;
            }

            DeviceProfile device =
                _devices[index];

            _selectedDeviceIndex = index;

            txtName.Text = device.Name;

            cboFunction.SelectedItem =
                device.FunctionCode.ToString("X2");

            txtRegister.Text =
                FormatRegister(device.RegisterAddress);

            txtCurrentAddress.Text =
                FormatAddress(device.CurrentAddress);

            txtNewAddress.Text =
                FormatAddress(device.NewAddress);

            Log(
                "已加载设备：" +
                device.Name);
        }

        private void RefreshDeviceGrid()
        {
            if (dgvDevices == null)
            {
                return;
            }

            dgvDevices.Rows.Clear();

            foreach (DeviceProfile device in _devices)
            {
                dgvDevices.Rows.Add(
                    device.Name,
                    device.FunctionCode.ToString("X2"),
                    FormatRegister(device.RegisterAddress),
                    FormatAddress(device.CurrentAddress),
                    FormatAddress(device.NewAddress),
                    device.Status,
                    device.LastOperationTime);
            }
        }

        private string FormatRegister(ushort register)
        {
            switch (_displayFormat)
            {
                case DisplayFormat.Hex:
                    return register.ToString("X4");

                case DisplayFormat.Binary:
                    return Convert.ToString(
                        register,
                        2).PadLeft(16, '0');

                default:
                    return register.ToString();
            }
        }

        private bool IsNewAddressUsedByOtherDevice(
            byte newAddress,
            int exceptIndex)
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                if (i == exceptIndex)
                {
                    continue;
                }

                if (_devices[i].NewAddress == newAddress)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 读取全部

        private void btnReadAll_Click(object sender, EventArgs e)
        {
            if (_devices.Count == 0)
            {
                MessageBox.Show(
                    "设备列表为空。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            foreach (DeviceProfile device in _devices)
            {
                try
                {
                    Log(
                        string.Format(
                            "读取设备：{0}，地址={1}",
                            device.Name,
                            FormatAddress(device.CurrentAddress)));

                    ushort value =
                        ReadRegister(
                            device.CurrentAddress,
                            device.RegisterAddress);

                    if (value < 1 || value > 247)
                    {
                        device.MarkStatus(
                            "读取成功，但地址值非法：" +
                            value);
                    }
                    else
                    {
                        device.CurrentAddress =
                            (byte)value;

                        device.MarkStatus(
                            "读取成功：" +
                            FormatAddress((byte)value));
                    }

                    Log(
                        string.Format(
                            "{0} 读取结果：{1}",
                            device.Name,
                            FormatAddress((byte)value)));
                }
                catch (Exception ex)
                {
                    device.MarkStatus(
                        "读取失败：" +
                        ex.Message);

                    Log(
                        string.Format(
                            "{0} 读取失败：{1}",
                            device.Name,
                            ex.Message));
                }

                RefreshDeviceGrid();

                Application.DoEvents();

                Thread.Sleep(100);
            }

            MessageBox.Show(
                "全部读取完成。",
                "完成",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        #region 修改全部

        private void btnWriteAll_Click(object sender, EventArgs e)
        {
            if (_devices.Count == 0)
            {
                MessageBox.Show(
                    "设备列表为空。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            List<string> duplicateAddresses =
                _devices
                    .GroupBy(x => x.NewAddress)
                    .Where(x => x.Count() > 1)
                    .Select(x => FormatAddress(x.Key))
                    .ToList();

            if (duplicateAddresses.Count > 0)
            {
                MessageBox.Show(
                    "发现重复的新地址：\r\n\r\n" +
                    string.Join(
                        ", ",
                        duplicateAddresses) +
                    "\r\n\r\n请先修改重复地址，否则可能造成总线冲突。",
                    "地址冲突",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            DialogResult confirm =
                MessageBox.Show(
                    "确定批量修改全部设备地址吗？\r\n\r\n" +
                    "程序会逐台执行：\r\n" +
                    "1. 使用当前地址读取\r\n" +
                    "2. 写入新地址\r\n" +
                    "3. 切换到新地址\r\n" +
                    "4. 重新读取验证\r\n" +
                    "5. 记录结果",
                    "批量修改确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            int success = 0;
            int failed = 0;

            for (int i = 0; i < _devices.Count; i++)
            {
                DeviceProfile device =
                    _devices[i];

                try
                {
                    Log(
                        "================================");

                    Log(
                        string.Format(
                            "开始修改：{0}",
                            device.Name));

                    Log(
                        string.Format(
                            "地址：{0} → {1}",
                            FormatAddress(device.CurrentAddress),
                            FormatAddress(device.NewAddress)));

                    // 1. 先读取当前寄存器
                    ushort currentValue =
                        ReadRegister(
                            device.CurrentAddress,
                            device.RegisterAddress);

                    Log(
                        string.Format(
                            "读取当前地址寄存器值：{0}",
                            FormatAddress((byte)currentValue)));

                    // 如果设备实际寄存器地址和配置不一致
                    if (currentValue != device.CurrentAddress)
                    {
                        Log(
                            "警告：寄存器中的当前地址与设备列表中的当前地址不同。");

                        device.CurrentAddress =
                            (byte)currentValue;
                    }

                    // 2. 写入
                    WriteRegister(
                        device.CurrentAddress,
                        device.RegisterAddress,
                        device.FunctionCode,
                        device.NewAddress);

                    Log("写入新地址成功。");

                    // 3. 等待设备保存
                    Thread.Sleep(150);

                    // 4. 使用新地址验证
                    ushort verifyValue =
                        ReadRegister(
                            device.NewAddress,
                            device.RegisterAddress);

                    if (verifyValue != device.NewAddress)
                    {
                        throw new InvalidDataException(
                            string.Format(
                                "验证失败：返回 {0}，期望 {1}",
                                verifyValue,
                                device.NewAddress));
                    }

                    device.CurrentAddress =
                        device.NewAddress;

                    device.MarkStatus(
                        "修改成功");

                    success++;

                    Log(
                        string.Format(
                            "修改成功并验证通过：{0}",
                            device.Name));
                }
                catch (Exception ex)
                {
                    device.MarkStatus(
                        "失败：" + ex.Message);

                    failed++;

                    Log(
                        string.Format(
                            "修改失败：{0} - {1}",
                            device.Name,
                            ex.Message));
                }

                RefreshDeviceGrid();

                Application.DoEvents();

                Thread.Sleep(200);
            }

            Log("================================");
            Log(
                string.Format(
                    "批量修改完成：成功 {0} 台，失败 {1} 台。",
                    success,
                    failed));

            MessageBox.Show(
                string.Format(
                    "批量修改完成！\r\n\r\n成功：{0} 台\r\n失败：{1} 台",
                    success,
                    failed),
                "完成",
                MessageBoxButtons.OK,
                failed == 0
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);
        }

        #endregion

        #region 导入导出 JSON

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_devices.Count == 0)
            {
                MessageBox.Show(
                    "没有设备可以导出。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog dialog =
                   new SaveFileDialog())
            {
                dialog.Filter =
                    "JSON 文件 (*.json)|*.json";

                dialog.FileName =
                    "Modbus设备地址配置.json";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(
                            typeof(List<DeviceProfile>));

                    using (FileStream stream =
                           new FileStream(
                               dialog.FileName,
                               FileMode.Create,
                               FileAccess.Write))
                    {
                        serializer.WriteObject(
                            stream,
                            _devices);
                    }

                    Log(
                        "设备配置已导出：" +
                        dialog.FileName);

                    MessageBox.Show(
                        "导出成功。",
                        "完成",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "导出失败：\r\n" + ex.Message,
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
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
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(
                            typeof(List<DeviceProfile>));

                    List<DeviceProfile> imported;

                    using (FileStream stream =
                           new FileStream(
                               dialog.FileName,
                               FileMode.Open,
                               FileAccess.Read))
                    {
                        imported =
                            serializer.ReadObject(stream)
                            as List<DeviceProfile>;
                    }

                    if (imported == null)
                    {
                        throw new InvalidDataException(
                            "JSON 文件内容无效。");
                    }

                    _devices.Clear();

                    foreach (DeviceProfile device in imported)
                    {
                        if (device.FunctionCode != 0x06 &&
                            device.FunctionCode != 0x10)
                        {
                            device.FunctionCode = 0x06;
                        }

                        if (device.CurrentAddress < 1 ||
                            device.CurrentAddress > 247)
                        {
                            device.CurrentAddress = 1;
                        }

                        if (device.NewAddress < 1 ||
                            device.NewAddress > 247)
                        {
                            device.NewAddress = 2;
                        }

                        _devices.Add(device);
                    }

                    RefreshDeviceGrid();

                    Log(
                        string.Format(
                            "已导入 {0} 台设备：{1}",
                            _devices.Count,
                            dialog.FileName));

                    MessageBox.Show(
                        string.Format(
                            "导入成功，共 {0} 台设备。",
                            _devices.Count),
                        "完成",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "导入失败：\r\n" + ex.Message,
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region CSV

        private void btnExportCsv_Click(
            object sender,
            EventArgs e)
        {
            if (_devices.Count == 0)
            {
                MessageBox.Show(
                    "没有设备可以导出。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog dialog =
                   new SaveFileDialog())
            {
                dialog.Filter =
                    "CSV 文件 (*.csv)|*.csv";

                dialog.FileName =
                    "Modbus地址修改记录.csv";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    StringBuilder builder =
                        new StringBuilder();

                    builder.AppendLine(
                        "名称,功能码,寄存器地址,当前地址,修改地址,状态,时间");

                    foreach (DeviceProfile device in _devices)
                    {
                        builder.AppendLine(
                            string.Join(
                                ",",
                                Csv(device.Name),
                                device.FunctionCode.ToString("X2"),
                                Csv(FormatRegister(
                                    device.RegisterAddress)),
                                Csv(FormatAddress(
                                    device.CurrentAddress)),
                                Csv(FormatAddress(
                                    device.NewAddress)),
                                Csv(device.Status),
                                Csv(device.LastOperationTime)));
                    }

                    File.WriteAllText(
                        dialog.FileName,
                        builder.ToString(),
                        new UTF8Encoding(true));

                    Log(
                        "修改记录已导出：" +
                        dialog.FileName);

                    MessageBox.Show(
                        "CSV 导出成功。",
                        "完成",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "CSV 导出失败：\r\n" +
                        ex.Message,
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private static string Csv(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }

            return "\"" +
                   value.Replace("\"", "\"\"") +
                   "\"";
        }

        #endregion

        #region 日志

        private void Log(string message)
        {
            if (txtLog == null)
            {
                return;
            }

            string line =
                string.Format(
                    "[{0}] {1}",
                    DateTime.Now.ToString("HH:mm:ss.fff"),
                    message);

            txtLog.AppendText(
                line +
                Environment.NewLine);

            txtLog.SelectionStart =
                txtLog.TextLength;

            txtLog.ScrollToCaret();
        }

        private static string BytesToString(byte[] data)
        {
            if (data == null ||
                data.Length == 0)
            {
                return "";
            }

            return string.Join(
                " ",
                data.Select(
                    b => b.ToString("X2")));
        }

        #endregion
    }
}
