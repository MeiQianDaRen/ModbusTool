using System;
using System.Runtime.Serialization;

namespace ModbusAddressTool
{
    [DataContract]
    public class DeviceProfile
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public byte ReadFunctionCode { get; set; }

        [DataMember(Order = 3)]
        public byte WriteFunctionCode { get; set; }

        [DataMember(Order = 4)]
        public ushort RegisterAddress { get; set; }

        [DataMember(Order = 5)]
        public byte CurrentAddress { get; set; }

        [DataMember(Order = 6)]
        public byte NewAddress { get; set; }

        [DataMember(Order = 7)]
        public ushort RegisterCount { get; set; }

        [DataMember(Order = 8)]
        public string DataType { get; set; }

        [DataMember(Order = 9)]
        public string Status { get; set; }

        [DataMember(Order = 10)]
        public string LastOperationTime { get; set; }

        // ============================================================
        // 厂家自定义数据帧
        // ============================================================

        /// <summary>
        /// 自定义读取数据帧。
        ///
        /// 例如：
        /// 01 03 00 D0 00 01
        ///
        /// 如果为空，则使用标准 Modbus RTU 自动生成。
        /// </summary>
        [DataMember(Order = 11)]
        public string CustomReadFrame { get; set; }

        /// <summary>
        /// 自定义修改数据帧。
        ///
        /// 例如：
        /// 01 41 00 D0 02
        ///
        /// 如果填写，则修改地址时完全按照该数据帧发送。
        /// </summary>
        [DataMember(Order = 12)]
        public string CustomWriteFrame { get; set; }

        /// <summary>
        /// 是否使用自定义修改数据帧。
        /// </summary>
        [DataMember(Order = 13)]
        public bool UseCustomWriteFrame { get; set; }

        /// <summary>
        /// 是否自动计算 CRC16。
        ///
        /// true：
        /// 输入数据帧不带 CRC，程序自动追加 CRC。
        ///
        /// false：
        /// 输入什么就发送什么。
        /// </summary>
        [DataMember(Order = 14)]
        public bool AutoAppendCrc { get; set; }

        /// <summary>
        /// 修改完成后是否自动验证。
        /// </summary>
        [DataMember(Order = 15)]
        public bool VerifyAfterWrite { get; set; }

        /// <summary>
        /// 最后发送的数据帧。
        /// </summary>
        [DataMember(Order = 16)]
        public string LastTxFrame { get; set; }

        /// <summary>
        /// 最后接收到的数据帧。
        /// </summary>
        [DataMember(Order = 17)]
        public string LastRxFrame { get; set; }

        public DeviceProfile()
        {
            Name = "新设备";

            ReadFunctionCode = 3;

            WriteFunctionCode = 6;

            RegisterAddress = 0;

            CurrentAddress = 1;

            NewAddress = 2;

            RegisterCount = 1;

            DataType = "UInt16";

            Status = "";

            LastOperationTime = "";

            CustomReadFrame = "";

            CustomWriteFrame = "";

            UseCustomWriteFrame = false;

            AutoAppendCrc = true;

            VerifyAfterWrite = true;

            LastTxFrame = "";

            LastRxFrame = "";
        }

        public DeviceProfile Clone()
        {
            return new DeviceProfile
            {
                Name = Name,

                ReadFunctionCode =
                    ReadFunctionCode,

                WriteFunctionCode =
                    WriteFunctionCode,

                RegisterAddress =
                    RegisterAddress,

                CurrentAddress =
                    CurrentAddress,

                NewAddress =
                    NewAddress,

                RegisterCount =
                    RegisterCount,

                DataType =
                    DataType,

                Status =
                    Status,

                LastOperationTime =
                    LastOperationTime,

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
                    LastRxFrame
            };
        }

        public void MarkStatus(
            string status)
        {
            Status = status;

            LastOperationTime =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss");
        }
    }
}
