using nexRemoteFree.Shared.Enums;
using System.Runtime.Serialization;

namespace nexRemoteFree.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class AudioSampleDto : BaseDto
    {
        public AudioSampleDto(byte[] buffer)
        {
            Buffer = buffer;
        }

        [DataMember(Name = "Buffer")]
        public byte[] Buffer { get; }


        [DataMember(Name = "DtoType")]
        public override BaseDtoType DtoType { get; init; } = BaseDtoType.AudioSample;

    }
}
