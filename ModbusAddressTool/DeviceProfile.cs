using System;
using System.Runtime.Serialization;

namespace ModbusAddressTool
{
    [DataContract]
    public class DeviceProfile
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// 读取功能码。
        /// 默认 03。
        /// </summary>
        [DataMember(Order = 2)]
        public byte ReadFunctionCode { get; set; }

        /// <summary>
        /// 写入功能码。
        /// 默认 06。
        /// </summary>
        [DataMember(Order = 3)]
        public byte WriteFunctionCode { get; set; }

        /// <summary>
        /// 地址所在寄存器。
        /// </summary>
        [DataMember(Order = 4)]
        public ushort RegisterAddress { get; set; }

        /// <summary>
        /// 当前设备地址。
        /// </summary>
        [DataMember(Order = 5)]
        public byte CurrentAddress { get; set; }

        /// <summary>
        /// 修改后的设备地址。
        /// </summary>
        [DataMember(Order = 6)]
        public byte NewAddress { get; set; }

        /// <summary>
        /// 每次读写使用的寄存器数量。
        /// 默认 1。
        /// </summary>
        [DataMember(Order = 7)]
        public ushort RegisterCount { get; set; }

        /// <summary>
        /// 读取数据类型。
        /// Address = 从寄存器读取一个 UInt16，并转换成地址。
        /// </summary>
        [DataMember(Order = 8)]
        public string DataType { get; set; }

        [DataMember(Order = 9)]
        public string Status { get; set; }

        [DataMember(Order = 10)]
        public string LastOperationTime { get; set; }

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
        }

        public DeviceProfile Clone()
        {
            return new DeviceProfile
            {
                Name = Name,
                ReadFunctionCode = ReadFunctionCode,
                WriteFunctionCode = WriteFunctionCode,
                RegisterAddress = RegisterAddress,
                CurrentAddress = CurrentAddress,
                NewAddress = NewAddress,
                RegisterCount = RegisterCount,
                DataType = DataType,
                Status = Status,
                LastOperationTime = LastOperationTime
            };
        }

        public void MarkStatus(string status)
        {
            Status = status;

            LastOperationTime =
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
