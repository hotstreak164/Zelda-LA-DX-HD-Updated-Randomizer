using System;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using ProjectZ.Base;
using ProjectZ.InGame.Things;
using ProjectZ.InGame.Controls;
using Microsoft.Xna.Framework;

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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var window = Window;
            if (window != null)
            {
                window.AddFlags(WindowManagerFlags.Fullscreen);
                window.AddFlags(WindowManagerFlags.LayoutNoLimits);
                window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);

                if (OperatingSystem.IsAndroidVersionAtLeast(28) && window.Attributes is { } attributes)
                    attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
            }

            base.OnCreate(savedInstanceState);

            var root = Application.Context.GetExternalFilesDir(null)!.AbsolutePath;

            // Point Values to a writable location on Android.
            Values.SetUserDataRoot(root);

            // Ensure folders exist.
            System.IO.Directory.CreateDirectory(Values.PathMods);
            System.IO.Directory.CreateDirectory(Values.PathLAHDMods);
            System.IO.Directory.CreateDirectory(Values.PathGraphicsMods);
            System.IO.Directory.CreateDirectory(Values.PathSaveFolder);

            // Get real display size for proper fullscreen rendering.
            var surfaceWidth = 0;
            var surfaceHeight = 0;

            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                var metrics = WindowManager?.CurrentWindowMetrics;
                var bounds = metrics?.Bounds;
                if (bounds != null)
                {
                    surfaceWidth = bounds.Width();
                    surfaceHeight = bounds.Height();
                }
            }
            else
            {
                var display = WindowManager?.DefaultDisplay;
                if (display != null)
                {
                    var size = new global::Android.Graphics.Point();
                #pragma warning disable CS0618
                    display.GetRealSize(size);
                #pragma warning restore CS0618
                    surfaceWidth = size.X;
                    surfaceHeight = size.Y;
                }
            }

            if (surfaceWidth > 0 && surfaceHeight > 0)
            {
                // Ensure landscape orientation (wider dimension first).
                if (surfaceWidth < surfaceHeight)
                {
                    var swap = surfaceWidth;
                    surfaceWidth = surfaceHeight;
                    surfaceHeight = swap;
                }
                Game1.SetAndroidSurfaceSizeHint(surfaceWidth, surfaceHeight);
            }

            // construct your real game here:
            var game = new Game1(
                editorMode: false,
                loadSave: false,
                loadSlot: 0
            );
            game.Services.AddService(typeof(AssetManager), Assets);

            var view = (View)game.Services.GetService(typeof(View))!;
            var matchParent = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);
            view.LayoutParameters = matchParent;
            SetContentView(view, matchParent);

            ApplyFullscreenFlags();

            view.Focusable = true;
            view.FocusableInTouchMode = true;
            view.RequestFocus();
            game.Run();
        }

        private void ApplyFullscreenFlags()
        {
            var window = Window;
            if (window == null)
                return;

            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                window.SetDecorFitsSystemWindows(false);
                var controller = window.InsetsController;
                if (controller != null)
                {
                    controller.Hide(global::Android.Views.WindowInsets.Type.StatusBars() |
                                   global::Android.Views.WindowInsets.Type.NavigationBars());
                    controller.SystemBarsBehavior =
                        (int)global::Android.Views.WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                }
            }
            else
            {
                var decorView = window.DecorView;
                if (decorView == null)
                    return;

            #pragma warning disable CS0618
                decorView.SystemUiVisibility =
                    (StatusBarVisibility)(
                        SystemUiFlags.LayoutStable |
                        SystemUiFlags.LayoutHideNavigation |
                        SystemUiFlags.LayoutFullscreen |
                        SystemUiFlags.HideNavigation |
                        SystemUiFlags.Fullscreen |
                        SystemUiFlags.ImmersiveSticky);
            #pragma warning restore CS0618
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
                ApplyFullscreenFlags();
        }

        public override bool DispatchKeyEvent(KeyEvent? e)
        {
            if (e == null)
                return base.DispatchKeyEvent(e);

            // Ignore key repeats - we do our own held-state tracking via BeginFrame().
            if (e.RepeatCount > 0)
                return base.DispatchKeyEvent(e);

            bool isDown = e.Action == KeyEventActions.Down;
            bool isUp   = e.Action == KeyEventActions.Up;
            if (!isDown && !isUp)
                return base.DispatchKeyEvent(e);

            // Map Android keycodes to CButtons for devices that route physical
            // buttons as KeyEvents instead of through the GamePad API.
            CButtons? mapped = e.KeyCode switch
            {
                Keycode.ButtonA      => CButtons.A,
                Keycode.ButtonB      => CButtons.B,
                Keycode.ButtonX      => CButtons.X,
                Keycode.ButtonY      => CButtons.Y,
                Keycode.ButtonL1     => CButtons.LB,
                Keycode.ButtonR1     => CButtons.RB,
                Keycode.ButtonL2     => CButtons.LT,
                Keycode.ButtonR2     => CButtons.RT,
                Keycode.ButtonStart  => CButtons.Start,
                Keycode.DpadUp       => CButtons.Up,
                Keycode.DpadDown     => CButtons.Down,
                Keycode.DpadLeft     => CButtons.Left,
                Keycode.DpadRight    => CButtons.Right,
                Keycode.ButtonThumbl => CButtons.LS,
                Keycode.ButtonThumbr => CButtons.RS,
                _                    => null
            };

            if (mapped.HasValue)
            {
                if ((e.Source & InputSourceType.Gamepad) != 0)
                    return base.DispatchKeyEvent(e);

                PlatformInput.SetKeyEventButton(mapped.Value, isDown);
                return true;
            }

            // Legacy select/back handling. Also feeds the new system so ButtonDown(Select) works.
            if (e.KeyCode == Keycode.Back        ||
                e.KeyCode == Keycode.ButtonSelect ||
                e.KeyCode == Keycode.ButtonMode  ||
                e.KeyCode == Keycode.Menu        ||
                e.KeyCode == Keycode.Escape)
            {
                PlatformInput.SetKeyEventButton(CButtons.Select, isDown);
                if (isDown)
                    PlatformInput.SelectPressed = true;
                return true;
            }
            return base.DispatchKeyEvent(e);
        }

        public override bool DispatchGenericMotionEvent(MotionEvent? e)
        {
            if (e == null)
                return base.DispatchGenericMotionEvent(e);

            // Read right stick axes directly for devices that report them via 
            // motion events but whose source flags MonoGame doesn't recognize.
            // AXIS_Z (11) = right stick X, AXIS_RZ (14) = right stick Y.
            float x = e.GetAxisValue(Axis.Z);
            float y = e.GetAxisValue(Axis.Rz);

            if (Math.Abs(x) > 0.05f || Math.Abs(y) > 0.05f || 
                PlatformInput.KeyEventRightStick != Vector2.Zero)
            {
                PlatformInput.SetKeyEventRightStick(x, y);
            }
            return base.DispatchGenericMotionEvent(e);
        }

        public override void OnBackPressed()
        {
            PlatformInput.SelectPressed = true;
            PlatformInput.SetKeyEventButton(CButtons.Select, true);
        }
    }
}