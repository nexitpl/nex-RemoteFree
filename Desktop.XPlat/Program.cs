using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using nexRemoteFree.Desktop.Core;
using nexRemoteFree.Shared.Utilities;
using System;
using System.Threading;

namespace nexRemoteFree.Desktop.XPlat
{
    class Program
    {

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();

        public static Conductor Conductor { get; private set; }
        public static IServiceProvider Services => ServiceContainer.Instance;

        public static CancellationTokenSource AppCts { get; private set; }
        public static CancellationToken AppCancellationToken { get; private set; }

        public static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                BuildAvaloniaApp().Start(AppMain, args);

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                throw;
            }
        }

        private static void AppMain(Application app, string[] args)
        {
            AppCts = new CancellationTokenSource();
            AppCancellationToken = AppCts.Token;
            app.Run(AppCancellationToken);
        }


        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Write((Exception)e.ExceptionObject);
        }
    }
}
