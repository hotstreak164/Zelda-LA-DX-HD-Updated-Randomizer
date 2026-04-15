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

    public enum NotificationType { Save, Reset }

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

    public void HideNotifications()
    {
        _savedTimer?.Stop();
        SavedNotification.Opacity = 0;
        ResetNotification.Opacity = 0;
    }

    public void ShowNotification(NotificationType type)
    {
        // Stop any existing saved timer.
        _savedTimer?.Stop();

        // Show the proper notification type.
        if (type == NotificationType.Save)
        {
            ResetNotification.Opacity = 0;
            SavedNotification.Opacity = 1.0;
        }
        else if (type == NotificationType.Reset)
        {
            ResetNotification.Opacity = 1.0;
            SavedNotification.Opacity = 0;
        }
        // Used to fade out the notification.
        double opacity = 1.0;
        int holdFrames = 60;
        int frameCount = 0;

        // Timer that fades out the message.
        _savedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _savedTimer.Tick += (s, e) =>
        {
            // Increment the framecount every 16 milliseconds.
            frameCount++;
            if (frameCount < holdFrames)
                return;

            // Reduce the opacity.
            opacity -= 0.02;
            if (opacity <= 0)
            {
                opacity = 0;
                _savedTimer.Stop();
            }
            // Fade out the notification.
            if (type == NotificationType.Save)
                SavedNotification.Opacity = opacity;
            else if (type == NotificationType.Reset)
                ResetNotification.Opacity = opacity;
        };
        // Start the timer.
        _savedTimer.Start();
    }
}