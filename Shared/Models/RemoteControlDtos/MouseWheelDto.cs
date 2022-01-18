using nexRemoteFree.Shared.Enums;
using System.Runtime.Serialization;

namespace nexRemoteFree.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class MouseWheelDto : BaseDto
    {

        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.MouseWheel;

        [DataMember(Name = "DeltaX")]
        public double DeltaX { get; set; }

        [DataMember(Name = "DeltaY")]
        public double DeltaY { get; set; }
    }
}
