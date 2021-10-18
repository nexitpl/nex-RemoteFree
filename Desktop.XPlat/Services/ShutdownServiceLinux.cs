using Microsoft.Extensions.DependencyInjection;
using nexRemote.Desktop.Core;
using nexRemote.Desktop.Core.Interfaces;
using nexRemote.Desktop.Core.Services;
using nexRemote.Shared.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace nexRemote.Desktop.XPlat.Services
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
