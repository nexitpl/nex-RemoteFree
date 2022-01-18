using System.Threading.Tasks;

namespace nexRemoteFree.Desktop.Core.Interfaces
{
    public interface IChatClientService
    {
        Task StartChat(string requesterID, string organizationName);
    }
}
