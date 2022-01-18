using Microsoft.Extensions.DependencyInjection;
using nexRemoteFree.Desktop.Core;
using nexRemoteFree.Desktop.Core.Interfaces;
using nexRemoteFree.Desktop.Core.Services;
using nexRemoteFree.Shared.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace nexRemoteFree.Desktop.XPlat.Services
{
    public class ShutdownServiceLinux : IShutdownService
    {
        public async Task Shutdown()
        {
            Logger.Debug($"Kończę ID procesu {Environment.ProcessId}.");
            var casterSocket = ServiceContainer.Instance.GetRequiredService<ICasterSocket>();
            await casterSocket.DisconnectAllViewers();
            Environment.Exit(0);
        }
    }
}
