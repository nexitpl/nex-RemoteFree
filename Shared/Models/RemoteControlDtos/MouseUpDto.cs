using nexRemoteFree.Shared.Enums;
using System.Runtime.Serialization;

namespace nexRemoteFree.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class MouseUpDto : BaseDto
    {
        [DataMember(Name = "Button")]
        public int Button { get; set; }

        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.MouseUp;

        [DataMember(Name = "PercentX")]
        public double PercentX { get; set; }

        [DataMember(Name = "PercentY")]
        public double PercentY { get; set; }
    }
}
