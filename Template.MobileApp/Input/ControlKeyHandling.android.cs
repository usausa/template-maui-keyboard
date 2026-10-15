namespace Template.MobileApp.Input;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using Microsoft.Maui.Handlers;

using Template.MobileApp.Behaviors;

using DatePicker = Microsoft.Maui.Controls.DatePicker;
using TimePicker = Microsoft.Maui.Controls.TimePicker;

public static partial class ControlKeyHandling
{
    private static partial void ConfigurePlatform(MauiAppBuilder builder)
    {
        SliderHandler.Mapper.AppendToMapping(nameof(ControlKeyHandling), static (handler, _) => handler.PlatformView.SetOnKeyListener(SliderKeyListener.Instance));
        StepperHandler.Mapper.AppendToMapping(nameof(ControlKeyHandling), static (handler, _) => UpdateStepper(handler.PlatformView));
        ViewHandler.ViewMapper.AppendToMapping(Focus.DefaultHighlightProperty.PropertyName, UpdateDefaultHighlight);

        builder.ConfigureMauiHandlers(static handlers =>
        {
            handlers.AddHandler<DatePicker, KeypadDatePickerHandler>();
            handlers.AddHandler<TimePicker, KeypadTimePickerHandler>();
        });
    }

    // ------------------------------------------------------------
    // Slider
    // ------------------------------------------------------------

    // 左右キーの 1 回の量を端までに抑える(MAUI の SeekBar の最大値は int.MaxValue で、端を越える加算があふれて反対の端へ戻る)
    private sealed class SliderKeyListener : Java.Lang.Object, View.IOnKeyListener
    {
        public static SliderKeyListener Instance { get; } = new();

        public bool OnKey(View? v, Keycode keyCode, KeyEvent? e)
        {
            if ((v is SeekBar seekBar) && (e?.Action == KeyEventActions.Down) && (keyCode is Keycode.DpadLeft or Keycode.DpadRight))
            {
                var step = (int)Math.Ceiling(((double)seekBar.Max - seekBar.Min) / 20);
                var increase = (keyCode == Keycode.DpadRight) == (seekBar.LayoutDirection == LayoutDirection.Ltr);
                seekBar.KeyProgressIncrement = increase
                    ? Math.Min(step, seekBar.Max - seekBar.Progress)
                    : Math.Min(step, seekBar.Progress - seekBar.Min);
            }

            // 値は SeekBar のキーの処理で変える(MAUI はキーやタッチで変わった値だけを受け取る)
            return false;
        }
    }

    // ------------------------------------------------------------
    // Stepper
    // ------------------------------------------------------------

    // 中のボタンではなく Stepper 全体にフォーカスを置き、左キーで減らす側、右キーで増やす側のボタンを押す
    private static void UpdateStepper(ViewGroup stepper)
    {
        stepper.DescendantFocusability = DescendantFocusability.BlockDescendants;
        stepper.SetOnKeyListener(StepperKeyListener.Instance);
    }

    private sealed class StepperKeyListener : Java.Lang.Object, View.IOnKeyListener
    {
        public static StepperKeyListener Instance { get; } = new();

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

    // ------------------------------------------------------------
    // DatePicker / TimePicker
    // ------------------------------------------------------------

    // 日付・時刻のダイアログは Esc 1 回でキャンセルして閉じる(Android の既定では 1 回目はダイアログの中のフォーカスを外す)
    internal static void HandleCancelKey(Dialog dialog) => dialog.SetOnKeyListener(DialogCancelKeyListener.Instance);

    private sealed class DialogCancelKeyListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
    {
        public static DialogCancelKeyListener Instance { get; } = new();

        // Esc は押した時から受け取り、離した時にキャンセルする
        public bool OnKey(IDialogInterface? dialog, Keycode keyCode, KeyEvent? e)
        {
            if (keyCode != Keycode.Escape)
            {
                return false;
            }

            if (e?.Action == KeyEventActions.Up)
            {
                dialog?.Cancel();
            }

            return true;
        }
    }

    // ------------------------------------------------------------
    // Focus
    // ------------------------------------------------------------

    // Focus.DefaultHighlight を指定したコントロールだけ切り替える(入力欄はもともと重ねない)
    private static void UpdateDefaultHighlight(IViewHandler handler, IView view)
    {
        if ((view is BindableObject bindable) && bindable.IsSet(Focus.DefaultHighlightProperty) && (handler.PlatformView is View platformView))
        {
            platformView.DefaultFocusHighlightEnabled = Focus.GetDefaultHighlight(bindable);
        }
    }
}

public sealed class KeypadDatePickerHandler : DatePickerHandler
{
    protected override DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
    {
        var dialog = base.CreateDatePickerDialog(year, month, day);
        ControlKeyHandling.HandleCancelKey(dialog);
        return dialog;
    }
}

public sealed class KeypadTimePickerHandler : TimePickerHandler
{
    protected override TimePickerDialog CreateTimePickerDialog(int hour, int minute)
    {
        var dialog = base.CreateTimePickerDialog(hour, minute);
        ControlKeyHandling.HandleCancelKey(dialog);
        return dialog;
    }
}
