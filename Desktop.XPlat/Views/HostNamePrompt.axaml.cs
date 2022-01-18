using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using nexRemoteFree.Desktop.XPlat.ViewModels;
using nexRemoteFree.Desktop.XPlat.Views;

namespace nexRemoteFree.Desktop.XPlat.Views
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
