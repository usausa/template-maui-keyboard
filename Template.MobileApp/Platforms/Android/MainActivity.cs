#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;
using Android.Content.PM;
using Android.OS;

using AndroidX.Activity;

[Activity(
    Name = "template.mobileapp.MainActivity",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Navigation,
    ScreenOrientation = ScreenOrientation.Portrait)]
public sealed class MainActivity : MauiAppCompatActivity
{
    private readonly KeyInputDriver keyInputDriver = new();

    private BackPressedCallback? backPressedCallback;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // アプリ全体のオーバースクロールを無効にする
        Theme?.ApplyStyle(_Microsoft.Android.Resource.Designer.ResourceConstant.Style.NoOverScrollTheme, true);

        base.OnCreate(savedInstanceState);

        backPressedCallback = new BackPressedCallback(this);
        OnBackPressedDispatcher.AddCallback(this, backPressedCallback);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            backPressedCallback?.Dispose();
            backPressedCallback = null;
        }

        base.Dispose(disposing);
    }

    public override bool DispatchKeyEvent(Android.Views.KeyEvent? e)
    {
        if ((e is not null) && keyInputDriver.Process(e, CurrentFocus))
        {
            return true;
        }

        return base.DispatchKeyEvent(e);
    }

    private sealed class BackPressedCallback : OnBackPressedCallback
    {
        private readonly MainActivity activity;

        public BackPressedCallback(MainActivity activity)
            : base(true)
        {
            this.activity = activity;
        }

        public override void HandleOnBackPressed()
        {
            var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
            var page = windows is { Count: > 0 } ? windows[^1].Page : null;
            if (page?.SendBackButtonPressed() ?? false)
            {
                return;
            }

            Enabled = false;
            try
            {
                activity.OnBackPressedDispatcher.OnBackPressed();
            }
            finally
            {
                Enabled = true;
            }
        }
    }
}
