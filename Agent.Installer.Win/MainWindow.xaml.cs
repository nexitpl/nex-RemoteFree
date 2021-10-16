using Remotely.Agent.Installer.Win.Utilities;
using Remotely.Agent.Installer.Win.ViewModels;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Remotely.Agent.Installer.Win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (CommandLineParser.CommandLineArgs.ContainsKey("quiet"))
            {
                Hide();
                ShowInTaskbar = false;
                _ = new MainWindowViewModel().Init();
            }
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await (DataContext as MainWindowViewModel).Init();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ShowServerUrlHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "To jest adres URL hostowanego serwera nex-Remote.  Urządzenie połączy się z tym adresem URL.", 
                "Serwer URL", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        private void ShowOrganizationIdHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "To jest identyfikator Twojej organizacji na serwerze nex-Remote.  Ponieważ jeden serwer nex-Remote może obsłużyć wiele organizacji , " +
                "ten identyfikator należy podać, aby określić, kto powinien mieć dostęp."
                + Environment.NewLine + Environment.NewLine +
                "Możesz znaleźć ten identyfikator na karcie Organizacja w aplikacji internetowej.", 
                "ID Organizacji", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        private void ShowSupportShortcutHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Jeśli ta opcja zostanie wybrana, instalator utworzy na pulpicie skrót do wsparcia nex-Remote dla tego urządzenia.", 
                "Skrót nex-Remote",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
