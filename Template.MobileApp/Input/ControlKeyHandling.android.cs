namespace Template.MobileApp.Input;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using Microsoft.Maui.Handlers;

using DatePicker = Microsoft.Maui.Controls.DatePicker;
using TimePicker = Microsoft.Maui.Controls.TimePicker;

public static partial class ControlKeyHandling
{
    //--------------------------------------------------------------------------------
    // Configure
    //--------------------------------------------------------------------------------

    private static partial void ConfigurePlatform(MauiAppBuilder builder)
    {
        SliderHandler.Mapper.AppendToMapping(nameof(ControlKeyHandling), static (handler, _) => handler.PlatformView.SetOnKeyListener(SliderKeyListener.Instance));
        StepperHandler.Mapper.AppendToMapping(nameof(ControlKeyHandling), static (handler, _) => UpdateStepper(handler.PlatformView));
        ViewHandler.ViewMapper.AppendToMapping(KeyFocus.DefaultHighlightProperty.PropertyName, UpdateDefaultHighlight);

        builder.ConfigureMauiHandlers(static handlers =>
        {
            handlers.AddHandler<DatePicker, KeypadDatePickerHandler>();
            handlers.AddHandler<TimePicker, KeypadTimePickerHandler>();
        });
    }

    //--------------------------------------------------------------------------------
    // Slider
    //--------------------------------------------------------------------------------

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

            return false;
        }
    }

    //--------------------------------------------------------------------------------
    // Stepper
    //--------------------------------------------------------------------------------

    private static void UpdateStepper(ViewGroup stepper)
    {
        stepper.DescendantFocusability = DescendantFocusability.BlockDescendants;
        stepper.SetOnKeyListener(StepperKeyListener.Instance);
    }

    private sealed class StepperKeyListener : Java.Lang.Object, View.IOnKeyListener
    {
        public static StepperKeyListener Instance { get; } = new();

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

    //--------------------------------------------------------------------------------
    // DatePicker / TimePicker
    //--------------------------------------------------------------------------------

    internal static void HandleCancelKey(Dialog dialog) => dialog.SetOnKeyListener(DialogCancelKeyListener.Instance);

    private sealed class DialogCancelKeyListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
    {
        public static DialogCancelKeyListener Instance { get; } = new();

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

    //--------------------------------------------------------------------------------
    // Focus
    //--------------------------------------------------------------------------------

    private static void UpdateDefaultHighlight(IViewHandler handler, IView view)
    {
        if ((view is BindableObject bindable) && bindable.IsSet(KeyFocus.DefaultHighlightProperty) && (handler.PlatformView is View platformView))
        {
            platformView.DefaultFocusHighlightEnabled = KeyFocus.GetDefaultHighlight(bindable);
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
