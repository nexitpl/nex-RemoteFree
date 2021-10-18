using Avalonia.Controls;
using Avalonia.Threading;
using nexRemote.Desktop.Core.Interfaces;
using nexRemote.Desktop.XPlat.Views;

namespace nexRemote.Desktop.XPlat.Services
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
