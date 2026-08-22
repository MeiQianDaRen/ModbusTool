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
        public byte FunctionCode { get; set; }

        [DataMember(Order = 3)]
        public ushort RegisterAddress { get; set; }

        [DataMember(Order = 4)]
        public byte CurrentAddress { get; set; }

        [DataMember(Order = 5)]
        public byte NewAddress { get; set; }

        [DataMember(Order = 6)]
        public string Status { get; set; }

        [DataMember(Order = 7)]
        public string LastOperationTime { get; set; }

        public DeviceProfile()
        {
            Name = "";
            FunctionCode = 6;
            RegisterAddress = 0;
            CurrentAddress = 1;
            NewAddress = 2;
            Status = "";
            LastOperationTime = "";
        }

        public DeviceProfile Clone()
        {
            return new DeviceProfile
            {
                Name = Name,
                FunctionCode = FunctionCode,
                RegisterAddress = RegisterAddress,
                CurrentAddress = CurrentAddress,
                NewAddress = NewAddress,
                Status = Status,
                LastOperationTime = LastOperationTime
            };
        }

        public void MarkStatus(string status)
        {
            Status = status;
            LastOperationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
