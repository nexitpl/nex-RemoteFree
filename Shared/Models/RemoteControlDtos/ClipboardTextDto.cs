using nexRemoteFree.Shared.Enums;
using System.Runtime.Serialization;

namespace nexRemoteFree.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class ClipboardTextDto : BaseDto
    {
        public ClipboardTextDto(string clipboardText)
        {
            ClipboardText = clipboardText;
        }

        [DataMember(Name = "ClipboardText")]
        public string ClipboardText { get; }


        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.ClipboardText;
    }
}
