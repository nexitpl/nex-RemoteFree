using nexRemote.Shared.Enums;
using System.Runtime.Serialization;

namespace nexRemote.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class MouseDownDto : BaseDto
    {
        [DataMember(Name = "Button")]
        public int Button { get; set; }

        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.MouseDown;

        [DataMember(Name = "PercentX")]
        public double PercentX { get; set; }

        [DataMember(Name = "PercentY")]
        public double PercentY { get; set; }
    }
}
