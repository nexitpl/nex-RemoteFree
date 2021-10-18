using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using nexRemote.Desktop.XPlat.ViewModels;
using nexRemote.Desktop.XPlat.Views;

namespace nexRemote.Desktop.XPlat.Views
{
    public class HostNamePrompt : Window
    {
        public HostNamePrompt()
        {
            Owner = MainWindow.Current;
            InitializeComponent();
        }

        public HostNamePromptViewModel ViewModel => DataContext as HostNamePromptViewModel;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
