using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace LADXHD_Launcher;

public partial class MainWindow : Window
{
    private HomeView _homeView;
    private SettingsView _settingsView;
    private ModsView _modsView;

    public HomeView HomeView => _homeView;
    public SettingsView SettingsView => _settingsView;
    public ModsView ModsView => _modsView;

    private DispatcherTimer _loadingTimer;
    private DispatcherTimer _savedTimer;

    public void NavigateTo(UserControl page)
    {
        PageContent.Content = page;
    }

    public MainWindow()
    {
        InitializeComponent();
        Config.Initialize();
        Config.LoadLauncherConfig();
        XnbAudio.Initialize();

        Height = Math.Clamp(App.SavedWindowHeight, 400, 768);

        CheckBox.IsCheckedChangedEvent.AddClassHandler<CheckBox>(
            (cb, e) => XnbAudio.PlayXnbSound(XnbAudio.SoundClick));
        ComboBox.SelectionChangedEvent.AddClassHandler<ComboBox>(
            (cb, e) => XnbAudio.PlayXnbSound(XnbAudio.SoundSelect));
        NumericUpDown.ValueChangedEvent.AddClassHandler<NumericUpDown>(
            (cb, e) => XnbAudio.PlayXnbSound(XnbAudio.SoundClick));

        XnbAudio.SuppressSound = true;
        _homeView = new HomeView(this);
        _settingsView = new SettingsView(this);
        _modsView = new ModsView(this);
        PageContent.Content = _homeView;
        XnbAudio.SuppressSound = false;

        this.SizeChanged += (s, e) => Config.SaveLauncherConfig();

        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == WindowStateProperty && WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
        };
    }

    public void ShowLoadingMessage()
    {
        LoadingNotification.Opacity = 1.0;
    }

    public void HideLoadingMessage()
    {
        double opacity = 1.0;
        _loadingTimer?.Stop();
        _loadingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _loadingTimer.Tick += (s, e) =>
        {
            opacity -= 0.05;
            if (opacity <= 0)
            {
                opacity = 0;
                _loadingTimer.Stop();
            }
            LoadingNotification.Opacity = opacity;
        };
        _loadingTimer.Start();
    }

    public void HideSavedNotification()
    {
        _savedTimer?.Stop();
        SavedNotification.Opacity = 0;
    }

    public void ShowSavedNotification()
    {
        // Stop any existing saved timer
        _savedTimer?.Stop();

        SavedNotification.Opacity = 1.0;
        double opacity = 1.0;
        int holdFrames = 60;
        int frameCount = 0;

        _savedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _savedTimer.Tick += (s, e) =>
        {
            frameCount++;
            if (frameCount < holdFrames)
                return;
            opacity -= 0.02;
            if (opacity <= 0)
            {
                opacity = 0;
                _savedTimer.Stop();
            }
            SavedNotification.Opacity = opacity;
        };
        _savedTimer.Start();
    }
}