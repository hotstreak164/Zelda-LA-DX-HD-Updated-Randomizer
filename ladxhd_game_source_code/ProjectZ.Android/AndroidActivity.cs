using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace ProjectZ.Android
{
    [Activity(
        Label = "ProjectZ",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges =
            ConfigChanges.Orientation |
            ConfigChanges.ScreenSize |
            ConfigChanges.KeyboardHidden |
            ConfigChanges.UiMode)]
    public class MainActivity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var game = new AndroidGame();
            var view = (View)game.Services.GetService(typeof(View))!;
            SetContentView(view);

            game.Run();
        }
    }
}