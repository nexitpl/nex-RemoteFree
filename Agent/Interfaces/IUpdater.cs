using System;
using System.Threading.Tasks;

namespace nexRemoteFree.Agent.Interfaces
{
    public interface IUpdater : IDisposable
    {
        Task BeginChecking();
        Task CheckForUpdates();
        Task InstallLatestVersion();
    }
}