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

    public bool Process(KeyEvent e, Window window)
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

        var focused = window.CurrentFocus;
        var route = ResolveRoute(entry, focused);
        if ((route == KeyRoute.Editor) && (focused is EditText))
        {
            return false;
        }

        if (route == KeyRoute.View)
        {
            // コントロールが処理するキー(入力欄の中のカーソルの移動など)はそちらを優先する
            if (window.SuperDispatchKeyEvent(e))
            {
                // 押し続けている途中からコントロールが処理した時も、離すまではコントロールに任せる
                consumedKeys.Remove(e.KeyCode);
                return true;
            }

            // 押し続けて端に来ても、入力欄やキーを処理していたコントロール(スライダーなど)の外へは出さない
            if ((e.RepeatCount > 0) && ((focused is EditText) || !consumedKeys.Contains(e.KeyCode)))
            {
                return true;
            }
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

    // 複数行の入力欄の中の上下は、行の移動を先にする
    private static KeyRoute ResolveRoute(KeyMapEntry entry, View? focused) =>
        (entry.Key is ShortcutKey.Up or ShortcutKey.Down) && (focused is EditText editText) && ((editText.InputType & InputTypes.TextFlagMultiLine) != 0)
            ? KeyRoute.View
            : entry.Route;
}
