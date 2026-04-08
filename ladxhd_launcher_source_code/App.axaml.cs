using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace LADXHD_Launcher
{
    public partial class App : Application
    {
        public static MainWindow? MainWindowInstance { get; private set; }
        public static double SavedWindowHeight { get; set; } = 768;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                MainWindowInstance = (MainWindow)desktop.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}