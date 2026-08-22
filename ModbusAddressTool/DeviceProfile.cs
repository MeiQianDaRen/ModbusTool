using System;
using System.Runtime.Serialization;

namespace ModbusAddressTool
{
    [DataContract]
    public class DeviceProfile
    {
        // ============================================================
        // 设备基本资料
        // ============================================================

        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public string Manufacturer { get; set; }

        [DataMember(Order = 3)]
        public string Model { get; set; }

        [DataMember(Order = 4)]
        public string Remark { get; set; }

        // ============================================================
        // 地址资料
        // ============================================================

        /// <summary>
        /// 最初记录的设备地址。
        /// 用于设备档案，不随着修改自动改变。
        /// </summary>
        [DataMember(Order = 10)]
        public byte OriginalAddress { get; set; }

        /// <summary>
        /// 当前认为设备使用的地址。
        /// </summary>
        [DataMember(Order = 11)]
        public byte CurrentAddress { get; set; }

        /// <summary>
        /// 准备修改成的地址。
        /// </summary>
        [DataMember(Order = 12)]
        public byte NewAddress { get; set; }

        // ============================================================
        // 串口参数
        // ============================================================

        [DataMember(Order = 20)]
        public string PortName { get; set; }

        [DataMember(Order = 21)]
        public int BaudRate { get; set; }

        [DataMember(Order = 22)]
        public string Parity { get; set; }

        [DataMember(Order = 23)]
        public int DataBits { get; set; }

        [DataMember(Order = 24)]
        public string StopBits { get; set; }

        // ============================================================
        // Modbus 参数
        // ============================================================

        [DataMember(Order = 30)]
        public byte ReadFunctionCode { get; set; }

        [DataMember(Order = 31)]
        public byte WriteFunctionCode { get; set; }

        [DataMember(Order = 32)]
        public ushort RegisterAddress { get; set; }

        [DataMember(Order = 33)]
        public ushort RegisterCount { get; set; }

        [DataMember(Order = 34)]
        public string DataType { get; set; }

        // ============================================================
        // 自定义数据帧
        // ============================================================

        /// <summary>
        /// 自定义读取数据帧。
        /// 例如：
        /// 01 03 00 D0 00 01
        /// </summary>
        [DataMember(Order = 40)]
        public string CustomReadFrame { get; set; }

        /// <summary>
        /// 自定义修改数据帧。
        /// 例如：
        /// 01 41 00 D0 02
        /// </summary>
        [DataMember(Order = 41)]
        public string CustomWriteFrame { get; set; }

        /// <summary>
        /// 是否启用自定义修改数据帧。
        /// </summary>
        [DataMember(Order = 42)]
        public bool UseCustomWriteFrame { get; set; }

        /// <summary>
        /// 是否自动追加 Modbus CRC16。
        /// </summary>
        [DataMember(Order = 43)]
        public bool AutoAppendCrc { get; set; }

        // ============================================================
        // 验证设置
        // ============================================================

        /// <summary>
        /// 修改成功后是否自动验证。
        /// </summary>
        [DataMember(Order = 50)]
        public bool VerifyAfterWrite { get; set; }

        // ============================================================
        // 最后一次操作
        // ============================================================

        [DataMember(Order = 60)]
        public string LastTxFrame { get; set; }

        [DataMember(Order = 61)]
        public string LastRxFrame { get; set; }

        [DataMember(Order = 62)]
        public string LastOperationTime { get; set; }

        [DataMember(Order = 63)]
        public string LastResult { get; set; }

        // ============================================================
        // 构造
        // ============================================================

        public DeviceProfile()
        {
            Name = "新设备";

            Manufacturer = "";

            Model = "";

            Remark = "";

            OriginalAddress = 1;

            CurrentAddress = 1;

            NewAddress = 2;

            PortName = "";

            BaudRate = 9600;

            Parity = "None";

            DataBits = 8;

            StopBits = "1";

            ReadFunctionCode = 3;

            WriteFunctionCode = 6;

            RegisterAddress = 0;

            RegisterCount = 1;

            DataType = "UInt16";

            CustomReadFrame = "";

            CustomWriteFrame = "";

            UseCustomWriteFrame = false;

            AutoAppendCrc = true;

            VerifyAfterWrite = true;

            LastTxFrame = "";

            LastRxFrame = "";

            LastOperationTime = "";

            LastResult = "";
        }

        // ============================================================
        // 状态
        // ============================================================

        public void MarkStatus(
            string status)
        {
            LastResult = status;

            LastOperationTime =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss");
        }

        // ============================================================
        // 复制
        // ============================================================

        public DeviceProfile Clone()
        {
            return new DeviceProfile
            {
                Name = Name,

                Manufacturer =
                    Manufacturer,

                Model =
                    Model,

                Remark =
                    Remark,

                OriginalAddress =
                    OriginalAddress,

                CurrentAddress =
                    CurrentAddress,

                NewAddress =
                    NewAddress,

                PortName =
                    PortName,

                BaudRate =
                    BaudRate,

                Parity =
                    Parity,

                DataBits =
                    DataBits,

                StopBits =
                    StopBits,

                ReadFunctionCode =
                    ReadFunctionCode,

                WriteFunctionCode =
                    WriteFunctionCode,

                RegisterAddress =
                    RegisterAddress,

                RegisterCount =
                    RegisterCount,

                DataType =
                    DataType,

                CustomReadFrame =
                    CustomReadFrame,

                CustomWriteFrame =
                    CustomWriteFrame,

                UseCustomWriteFrame =
                    UseCustomWriteFrame,

                AutoAppendCrc =
                    AutoAppendCrc,

                VerifyAfterWrite =
                    VerifyAfterWrite,

                LastTxFrame =
                    LastTxFrame,

                LastRxFrame =
                    LastRxFrame,

                LastOperationTime =
                    LastOperationTime,

                LastResult =
                    LastResult
            };
        }
    }
}
