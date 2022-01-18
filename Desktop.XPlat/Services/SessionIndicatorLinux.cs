using Avalonia.Controls;
using Avalonia.Threading;
using nexRemoteFree.Desktop.Core.Interfaces;
using nexRemoteFree.Desktop.XPlat.Views;

namespace nexRemoteFree.Desktop.XPlat.Services
{
    public class SessionIndicatorLinux : ISessionIndicator
    {
        public void Show()
        {
            Dispatcher.UIThread.Post(() =>
            {
                var indicatorWindow = new SessionIndicatorWindow();
                indicatorWindow.Show();
            });
        }
    }
}
