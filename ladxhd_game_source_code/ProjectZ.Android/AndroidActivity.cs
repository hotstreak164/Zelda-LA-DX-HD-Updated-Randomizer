using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using ProjectZ.Base;
using ProjectZ.InGame.Things;

namespace ProjectZ.Android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = false ,
        Theme = "@style/Theme.Game",
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullSensor,
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

            var root = Application.Context.GetExternalFilesDir(null)!.AbsolutePath;

            // Point Values to a writable location on Android.
            Values.SetUserDataRoot(root);

            // Ensure folders exist.
            System.IO.Directory.CreateDirectory(Values.PathMods);
            System.IO.Directory.CreateDirectory(Values.PathLAHDMods);
            System.IO.Directory.CreateDirectory(Values.PathGraphicsMods);
            System.IO.Directory.CreateDirectory(Values.PathSaveFolder);

            // construct your real game here:
            var game = new Game1(
                editorMode: false,
                loadSave: false,
                loadSlot: 0
            );
            game.Services.AddService(typeof(AssetManager), Assets);

            var view = (View)game.Services.GetService(typeof(View))!;
            SetContentView(view);

            view.Focusable = true;
            view.FocusableInTouchMode = true;
            view.RequestFocus();

            game.Run();
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            // Only treat "down" as a press (avoid repeats).
            if (e.Action == KeyEventActions.Down && e.RepeatCount == 0)
            {
                // Catch a wider net than just Back/Select.
                if (e.KeyCode == Keycode.Back ||
                    e.KeyCode == Keycode.ButtonSelect ||
                    e.KeyCode == Keycode.ButtonMode ||
                    e.KeyCode == Keycode.Menu ||
                    e.KeyCode == Keycode.Escape) // many controllers map select/back to ESC
                {
                    PlatformInput.SelectPressed = true;
                    return true;
                }
            }
            return base.DispatchKeyEvent(e);
        }

        public override void OnBackPressed()
        {
            PlatformInput.SelectPressed = true;
        }
    }
}