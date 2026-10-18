#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.Views;

using Template.MobileApp.Input;

public enum KeyTrigger
{
    Press,
    Release
}

public enum KeyRoute
{
    Handler,
    Editor,
    View
}

public sealed record KeyMapEntry(ShortcutKey Key, KeyTrigger Trigger, KeyRoute Route);

public static class KeyMap
{
    private static readonly Dictionary<Keycode, KeyMapEntry> Entries = new()
    {
        [Keycode.DpadUp] = new(ShortcutKey.Up, KeyTrigger.Press, KeyRoute.Handler),
        [Keycode.DpadDown] = new(ShortcutKey.Down, KeyTrigger.Press, KeyRoute.Handler),
        [Keycode.DpadLeft] = new(ShortcutKey.Left, KeyTrigger.Press, KeyRoute.View),
        [Keycode.DpadRight] = new(ShortcutKey.Right, KeyTrigger.Press, KeyRoute.View),
        [Keycode.Enter] = new(ShortcutKey.Enter, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.NumpadEnter] = new(ShortcutKey.Enter, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.DpadCenter] = new(ShortcutKey.Enter, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Escape] = new(ShortcutKey.Escape, KeyTrigger.Release, KeyRoute.Handler),
        [Keycode.Del] = new(ShortcutKey.Clear, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Clear] = new(ShortcutKey.Clear, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Minus] = new(ShortcutKey.Minus, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.NumpadSubtract] = new(ShortcutKey.Minus, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Period] = new(ShortcutKey.Period, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.NumpadDot] = new(ShortcutKey.Period, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num0] = new(ShortcutKey.Num0, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num1] = new(ShortcutKey.Num1, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num2] = new(ShortcutKey.Num2, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num3] = new(ShortcutKey.Num3, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num4] = new(ShortcutKey.Num4, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num5] = new(ShortcutKey.Num5, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num6] = new(ShortcutKey.Num6, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num7] = new(ShortcutKey.Num7, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num8] = new(ShortcutKey.Num8, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Num9] = new(ShortcutKey.Num9, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad0] = new(ShortcutKey.Num0, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad1] = new(ShortcutKey.Num1, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad2] = new(ShortcutKey.Num2, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad3] = new(ShortcutKey.Num3, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad4] = new(ShortcutKey.Num4, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad5] = new(ShortcutKey.Num5, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad6] = new(ShortcutKey.Num6, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad7] = new(ShortcutKey.Num7, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad8] = new(ShortcutKey.Num8, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.Numpad9] = new(ShortcutKey.Num9, KeyTrigger.Release, KeyRoute.Editor),
        [Keycode.F1] = new(ShortcutKey.Function1, KeyTrigger.Release, KeyRoute.Handler),
        [Keycode.F2] = new(ShortcutKey.Function2, KeyTrigger.Release, KeyRoute.Handler),
        [Keycode.F3] = new(ShortcutKey.Function3, KeyTrigger.Release, KeyRoute.Handler),
        [Keycode.F4] = new(ShortcutKey.Function4, KeyTrigger.Release, KeyRoute.Handler)
    };

    public static KeyMapEntry? Find(Keycode keycode) => Entries.GetValueOrDefault(keycode);
}
