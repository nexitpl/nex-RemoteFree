using nexRemote.Shared.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace nexRemote.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class ScreenDataDto : BaseDto
    {
        [DataMember(Name = "DisplayNames")]
        public IEnumerable<string> DisplayNames { get; init; }


        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.ScreenData;

        [DataMember(Name = "SelectedDisplay")]
        public string SelectedDisplay { get; init; }

        [DataMember(Name = "MachineName")]
        public string MachineName { get; init; }

        [DataMember(Name = "ScreenWidth")]
        public int ScreenWidth { get; init; }

        [DataMember(Name = "ScreenHeight")]
        public int ScreenHeight { get; init; }
    }
}
