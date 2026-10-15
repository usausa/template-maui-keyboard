namespace Template.MobileApp.Behaviors;

using Android.Views;

using Microsoft.Maui.Handlers;

public static partial class StepperOption
{
    private static readonly StepperKeyListener KeyListener = new();

    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        if (options.HandleArrowKey)
        {
            StepperHandler.Mapper.AppendToMapping(nameof(BehaviorOptions.HandleArrowKey), static (handler, _) => UpdateHandleArrowKey(handler.PlatformView));
        }
    }

    // 中のボタンではなく Stepper 全体にフォーカスを置き、左キーで減らす側、右キーで増やす側のボタンを押す
    private static void UpdateHandleArrowKey(ViewGroup stepper)
    {
        stepper.DescendantFocusability = DescendantFocusability.BlockDescendants;
        stepper.SetOnKeyListener(KeyListener);
    }

    private sealed class StepperKeyListener : Java.Lang.Object, View.IOnKeyListener
    {
        // 押すボタンが無効(値が端)の時は処理せず、隣のコントロールへ移す
        public bool OnKey(View? v, Keycode keyCode, KeyEvent? e)
        {
            if ((e?.Action != KeyEventActions.Down) || (v is not ViewGroup { ChildCount: 2 } stepper) || (keyCode is not (Keycode.DpadLeft or Keycode.DpadRight)))
            {
                return false;
            }

            var button = stepper.GetChildAt(keyCode == Keycode.DpadLeft ? 0 : 1);
            if (button is not { Enabled: true })
            {
                return false;
            }

            button.PerformClick();
            return true;
        }
    }
}
