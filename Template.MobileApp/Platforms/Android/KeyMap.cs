#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.Views;

using Template.MobileApp.Input;

public enum KeyTrigger
{
    // 押した時(押し続けると繰り返す)
    Press,
    // 離した時
    Release
}

public enum EditorKeyMode
{
    // 入力欄にフォーカスがあってもアプリで処理する
    Handle,
    // 入力欄に渡す
    PassThrough,
    // 入力欄に渡す(カーソルが端にある時は止める)
    Cursor
}

public sealed record KeyMapEntry(ShortcutKey Key, KeyTrigger Trigger, EditorKeyMode Editor);

public static class KeyMap
{
    // 端末のキーコード → 論理のキー
    private static readonly Dictionary<Keycode, KeyMapEntry> Entries = new()
    {
        [Keycode.DpadUp] = new(ShortcutKey.Up, KeyTrigger.Press, EditorKeyMode.Handle),
        [Keycode.DpadDown] = new(ShortcutKey.Down, KeyTrigger.Press, EditorKeyMode.Handle),
        [Keycode.DpadLeft] = new(ShortcutKey.Left, KeyTrigger.Press, EditorKeyMode.Cursor),
        [Keycode.DpadRight] = new(ShortcutKey.Right, KeyTrigger.Press, EditorKeyMode.Cursor),
        [Keycode.Enter] = new(ShortcutKey.Enter, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.NumpadEnter] = new(ShortcutKey.Enter, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.DpadCenter] = new(ShortcutKey.Enter, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Escape] = new(ShortcutKey.Escape, KeyTrigger.Release, EditorKeyMode.Handle),
        [Keycode.Del] = new(ShortcutKey.Clear, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Clear] = new(ShortcutKey.Clear, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Minus] = new(ShortcutKey.Minus, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.NumpadSubtract] = new(ShortcutKey.Minus, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Period] = new(ShortcutKey.Period, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.NumpadDot] = new(ShortcutKey.Period, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num0] = new(ShortcutKey.Num0, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num1] = new(ShortcutKey.Num1, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num2] = new(ShortcutKey.Num2, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num3] = new(ShortcutKey.Num3, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num4] = new(ShortcutKey.Num4, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num5] = new(ShortcutKey.Num5, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num6] = new(ShortcutKey.Num6, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num7] = new(ShortcutKey.Num7, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num8] = new(ShortcutKey.Num8, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Num9] = new(ShortcutKey.Num9, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad0] = new(ShortcutKey.Num0, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad1] = new(ShortcutKey.Num1, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad2] = new(ShortcutKey.Num2, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad3] = new(ShortcutKey.Num3, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad4] = new(ShortcutKey.Num4, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad5] = new(ShortcutKey.Num5, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad6] = new(ShortcutKey.Num6, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad7] = new(ShortcutKey.Num7, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad8] = new(ShortcutKey.Num8, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.Numpad9] = new(ShortcutKey.Num9, KeyTrigger.Release, EditorKeyMode.PassThrough),
        [Keycode.F1] = new(ShortcutKey.Function1, KeyTrigger.Release, EditorKeyMode.Handle),
        [Keycode.F2] = new(ShortcutKey.Function2, KeyTrigger.Release, EditorKeyMode.Handle),
        [Keycode.F3] = new(ShortcutKey.Function3, KeyTrigger.Release, EditorKeyMode.Handle),
        [Keycode.F4] = new(ShortcutKey.Function4, KeyTrigger.Release, EditorKeyMode.Handle)
    };

    public static KeyMapEntry? Find(Keycode keycode) => Entries.GetValueOrDefault(keycode);
}
