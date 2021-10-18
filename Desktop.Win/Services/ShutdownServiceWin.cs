using Microsoft.Extensions.DependencyInjection;
using nexRemote.Desktop.Core;
using nexRemote.Desktop.Core.Interfaces;
using nexRemote.Desktop.Core.Services;
using nexRemote.Shared.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace nexRemote.Desktop.Win.Services
{
    public class ShutdownServiceWin : IShutdownService
    {
        public async Task Shutdown()
        {
            try
            {
                Logger.Write($"Kończę ID procesu {Environment.ProcessId}.");
                var casterSocket = ServiceContainer.Instance.GetRequiredService<ICasterSocket>();
                await casterSocket.DisconnectAllViewers();
                await casterSocket.Disconnect();
                System.Windows.Forms.Application.Exit();
                App.Current.Dispatcher.Invoke(() =>
                {
                    App.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }
    }
}
