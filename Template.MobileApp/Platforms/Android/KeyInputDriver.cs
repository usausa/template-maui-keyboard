#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.Views;
using Android.Widget;

using Template.MobileApp.Input;

public sealed class KeyInputDriver
{
    private readonly HashSet<Keycode> consumedKeys = [];

    public bool Process(KeyEvent e, View? focused)
    {
        var entry = KeyMap.Find(e.KeyCode);
        if (entry is null)
        {
            return false;
        }

        // 押した時に受け取ったキーは、離した時も Android に渡さない
        if (e.Action == KeyEventActions.Up)
        {
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

        if ((focused is EditText editText) && (entry.Editor != EditorKeyMode.Handle))
        {
            return (entry.Editor == EditorKeyMode.Cursor) && IsCursorEdge(editText, entry.Key);
        }

        // 押し続けた時の繰り返し
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

        // 処理しないキーは Android に渡す
        var consumed = entry.Trigger == KeyTrigger.Press
            ? InputManager.Default.Process(entry.Key)
            : InputManager.Default.CanProcess(entry.Key);
        if (consumed)
        {
            consumedKeys.Add(e.KeyCode);
        }

        return consumed;
    }

    private static bool IsCursorEdge(EditText editText, ShortcutKey key)
    {
        var position = key == ShortcutKey.Left ? 0 : (editText.Text?.Length ?? 0);
        return (editText.SelectionStart == position) && (editText.SelectionEnd == position);
    }
}
