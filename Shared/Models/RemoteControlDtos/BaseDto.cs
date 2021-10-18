using nexRemote.Shared.Enums;
using System.Runtime.Serialization;

namespace nexRemote.Shared.Models.RemoteControlDtos
{
    [DataContract]
    public class BaseDto
    {
        [DataMember(Name = "DtoType")]
        public virtual BaseDtoType DtoType { get; init; }
    }
}
