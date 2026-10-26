#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware.Input;
using Android.Views;

using Java.Lang;

using KeyboardType = Android.Content.Res.KeyboardType;

internal static class HardKeyboard
{
    private static readonly int[] KeypadKeys =
    [
        (int)Keycode.Num0, (int)Keycode.Num1, (int)Keycode.Num2, (int)Keycode.Num3, (int)Keycode.Num4,
        (int)Keycode.Num5, (int)Keycode.Num6, (int)Keycode.Num7, (int)Keycode.Num8, (int)Keycode.Num9
    ];

    private static readonly KeyboardCallbacks Callbacks = new();

    private static readonly DeviceListener Listener = new();

    public static event EventHandler? Changed;

    public static bool SuppressSoftInput { get; private set; }

    public static void Start()
    {
        var context = Application.Context;
        SuppressSoftInput = HasHardKeyboard(context.Resources!.Configuration!);
        context.RegisterComponentCallbacks(Callbacks);
        (context.GetSystemService(Context.InputService) as InputManager)?.RegisterInputDeviceListener(Listener, null);
    }

    private static bool HasHardKeyboard(Configuration configuration) =>
        ((configuration.Keyboard != KeyboardType.Nokeys) && (configuration.HardKeyboardHidden != HardKeyboardHidden.Yes)) || HasKeypad();

    private static bool HasKeypad()
    {
        foreach (var id in InputDevice.GetDeviceIds() ?? [])
        {
            var device = InputDevice.GetDevice(id);
            if ((device is not null) && !device.IsVirtual && device.SupportsSource(InputSourceType.Keyboard) &&
                (device.HasKeys(KeypadKeys)?.All(static x => x) ?? false))
            {
                return true;
            }
        }

        return false;
    }

    private static void Update(Configuration configuration)
    {
        var value = HasHardKeyboard(configuration);
        if (value == SuppressSoftInput)
        {
            return;
        }

        SuppressSoftInput = value;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    private sealed class KeyboardCallbacks : Object, IComponentCallbacks
    {
        public void OnConfigurationChanged(Configuration newConfig) => Update(newConfig);

        public void OnLowMemory()
        {
        }
    }

    private sealed class DeviceListener : Object, InputManager.IInputDeviceListener
    {
        public void OnInputDeviceAdded(int deviceId) => Update(Application.Context.Resources!.Configuration!);

        public void OnInputDeviceChanged(int deviceId) => Update(Application.Context.Resources!.Configuration!);

        public void OnInputDeviceRemoved(int deviceId) => Update(Application.Context.Resources!.Configuration!);
    }
}
