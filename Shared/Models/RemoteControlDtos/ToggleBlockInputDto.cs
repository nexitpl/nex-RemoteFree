using nexRemote.Shared.Enums;
using System.Runtime.Serialization;

namespace nexRemote.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class ToggleBlockInputDto : BaseDto
    {
        [DataMember(Name = "ToggleOn")]
        public bool ToggleOn { get; set; }

        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.ToggleBlockInput;
    }
}
