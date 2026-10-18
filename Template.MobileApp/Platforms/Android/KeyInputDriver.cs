#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.Text;
using Android.Views;
using Android.Widget;

using Template.MobileApp.Input;

public sealed class KeyInputDriver
{
    private readonly HashSet<Keycode> consumedKeys = [];

    private readonly HashSet<Keycode> discardedKeys = [];

    public bool Process(KeyEvent e, Window window)
    {
        var entry = KeyMap.Find(e.KeyCode);
        if (entry is null)
        {
            return false;
        }

        // Key up
        if (e.Action == KeyEventActions.Up)
        {
            if (discardedKeys.Remove(e.KeyCode))
            {
                consumedKeys.Remove(e.KeyCode);
                return true;
            }

            if (!consumedKeys.Remove(e.KeyCode))
            {
                return false;
            }

            if ((entry.Trigger == KeyTrigger.Release) && !e.IsCanceled)
            {
                InputManager.Default.Process(entry.Key);
            }

            return true;
        }

        if (e.Action != KeyEventActions.Down)
        {
            return false;
        }

        // Discard while busy
        if (InputManager.Default.IsBusy)
        {
            discardedKeys.Add(e.KeyCode);
            return true;
        }

        discardedKeys.Remove(e.KeyCode);

        var focused = window.CurrentFocus;
        var route = ResolveRoute(entry, focused);
        if ((route == KeyRoute.Editor) && (focused is EditText))
        {
            return false;
        }

        if (route == KeyRoute.View)
        {
            // Control first
            if (window.SuperDispatchKeyEvent(e))
            {
                consumedKeys.Remove(e.KeyCode);
                return true;
            }

            if ((e.RepeatCount > 0) && ((focused is EditText) || !consumedKeys.Contains(e.KeyCode)))
            {
                return true;
            }
        }

        // Repeat
        if (e.RepeatCount > 0)
        {
            if (!consumedKeys.Contains(e.KeyCode))
            {
                return false;
            }

            if (entry.Trigger == KeyTrigger.Press)
            {
                InputManager.Default.Process(entry.Key);
            }

            return true;
        }

        // Handler
        var consumed = entry.Trigger == KeyTrigger.Press
            ? InputManager.Default.Process(entry.Key)
            : InputManager.Default.CanProcess(entry.Key);
        if (consumed)
        {
            consumedKeys.Add(e.KeyCode);
        }

        return consumed;
    }

    private static KeyRoute ResolveRoute(KeyMapEntry entry, View? focused) =>
        (entry.Key is ShortcutKey.Up or ShortcutKey.Down) && (focused is EditText editText) && ((editText.InputType & InputTypes.TextFlagMultiLine) != 0)
            ? KeyRoute.View
            : entry.Route;
}
