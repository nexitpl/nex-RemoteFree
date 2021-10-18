using System.Threading.Tasks;

namespace nexRemote.Desktop.Core.Interfaces
{
    public interface IChatClientService
    {
        Task StartChat(string requesterID, string organizationName);
    }
}
