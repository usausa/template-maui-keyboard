#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;
using Android.Content;
using Android.Content.Res;

using Java.Lang;

using KeyboardType = Android.Content.Res.KeyboardType;

internal static class HardKeyboard
{
    private static readonly KeyboardCallbacks Callbacks = new();

    public static event EventHandler? Changed;

    public static bool SuppressSoftInput { get; private set; }

    public static void Start()
    {
        SuppressSoftInput = HasHardKeyboard(Application.Context.Resources!.Configuration!);
        Application.Context.RegisterComponentCallbacks(Callbacks);
    }

    private static bool HasHardKeyboard(Configuration configuration) =>
        (configuration.Keyboard != KeyboardType.Nokeys) && (configuration.HardKeyboardHidden != HardKeyboardHidden.Yes);

    private sealed class KeyboardCallbacks : Object, IComponentCallbacks
    {
        public void OnConfigurationChanged(Configuration newConfig)
        {
            var value = HasHardKeyboard(newConfig);
            if (value == SuppressSoftInput)
            {
                return;
            }

            SuppressSoftInput = value;
            Changed?.Invoke(null, EventArgs.Empty);
        }

        public void OnLowMemory()
        {
        }
    }
}
