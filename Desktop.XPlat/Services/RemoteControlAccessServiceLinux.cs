using Avalonia.Threading;
using nexRemoteFree.Desktop.Core.Interfaces;
using nexRemoteFree.Desktop.XPlat.ViewModels;
using nexRemoteFree.Desktop.XPlat.Views;
using nexRemoteFree.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nexRemoteFree.Desktop.XPlat.Services
{
    public class RemoteControlAccessServiceLinux : IRemoteControlAccessService
    {
        public async Task<bool> PromptForAccess(string requesterName, string organizationName)
        {
            return await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var promptWindow = new PromptForAccessWindow();
                var viewModel = promptWindow.DataContext as PromptForAccessWindowViewModel;
                if (!string.IsNullOrWhiteSpace(requesterName))
                {
                    viewModel.RequesterName = requesterName;
                }
                if (!string.IsNullOrWhiteSpace(organizationName))
                {
                    viewModel.OrganizationName = organizationName;
                }

                var isOpen = true;
                promptWindow.Closed += (sender, arg) =>
                {
                    isOpen = false;
                };
                promptWindow.Show();
                while (isOpen)
                {
                    await Task.Delay(100);
                }
   
                return viewModel.PromptResult;
            });
        }
    }
}
