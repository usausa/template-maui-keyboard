#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;
using Android.Content.PM;
using Android.OS;

[Activity(
    Name = "template.mobileapp.MainActivity",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait)]
public sealed class MainActivity : MauiAppCompatActivity
{
    private KeyInputDriver keyInputDriver = default!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        keyInputDriver = new KeyInputDriver(this);
    }

    public override bool DispatchKeyEvent(Android.Views.KeyEvent? e)
    {
        if (keyInputDriver.Process(e!))
        {
            return true;
        }

        return base.DispatchKeyEvent(e);
    }
}
