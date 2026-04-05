using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;

namespace LADXHD_Launcher;

public partial class HomeView : UserControl
{
    private MainWindow? _parent;

    public HomeView()
    {
        InitializeComponent();
    }

    public HomeView(MainWindow parent)
    {
        InitializeComponent();
        SoundToggle_SetImage();
        _parent = parent;
    }

    private string GetGameDirectory()
    {
        // for now return the directory the launcher is running from
        return AppContext.BaseDirectory;
    }

    private void SoundToggle_SetImage()
    {
        SoundButtonImage.Source = XnbAudio.Enabled
            ? new Avalonia.Media.Imaging.Bitmap(
                AssetLoader.Open(new Uri("avares://Launcher/Resources/sound_on.png")))
            : new Avalonia.Media.Imaging.Bitmap(
                AssetLoader.Open(new Uri("avares://Launcher/Resources/sound_off.png")));
    }

    private void SoundToggle_Click(object sender, RoutedEventArgs e)
    {
        XnbAudio.Enabled = !XnbAudio.Enabled;
        Config.SaveLauncherConfig();
        SoundToggle_SetImage();
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(Config.ZeldaEXE))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = Config.ZeldaEXE,
            WorkingDirectory = Config.BaseFolder,
            UseShellExecute = true
        });
        _parent?.Close();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        int maxGameScale = AdvancedSettings.LoadMaxGameScale(GetGameDirectory());
        GameSettings.Load(GetGameDirectory());
        _parent?.SettingsView.LoadValues(maxGameScale);
        _parent?.NavigateTo(_parent.SettingsView);
        XnbAudio.PlayXnbSound(XnbAudio.SoundOpen);
    }

    private async void ModsButton_Click(object sender, RoutedEventArgs e)
    {
        _parent?.HideSavedNotification();
        _parent?.ShowLoadingMessage();

        await System.Threading.Tasks.Task.Run(() =>
        {
            AdvancedSettings.Load(AppContext.BaseDirectory);
        });

        _parent?.ModsView.LoadValues();
        _parent?.HideLoadingMessage();
        _parent?.NavigateTo(_parent.ModsView);

        // Wait for the UI to actually render before playing sound
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => XnbAudio.PlayXnbSound(XnbAudio.SoundOpen),
            Avalonia.Threading.DispatcherPriority.Background);
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        _parent?.Close();
    }
}