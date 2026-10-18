namespace Template.MobileApp.Components;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using Google.Android.Material.Dialog;

using Microsoft.Maui.Platform;

using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

public sealed partial class KeypadDialog
{
    public async partial ValueTask InformationAsync(string message, string? title, string ok)
    {
        using var dialog = new InformationDialog(ActivityResolver.CurrentActivity, config);
        await dialog.ShowAsync(message, title, ok).ConfigureAwait(true);
    }

    public async partial ValueTask<bool> ConfirmAsync(string message, string? title, string ok, string cancel, bool defaultPositive)
    {
        using var dialog = new ConfirmDialog(ActivityResolver.CurrentActivity, config);
        return await dialog.ShowAsync(message, title, ok, cancel, defaultPositive).ConfigureAwait(true);
    }

    public async partial ValueTask<Confirm3Result> Confirm3Async(string message, string? title, string ok, string cancel, string neutral, bool defaultPositive)
    {
        using var dialog = new Confirm3Dialog(ActivityResolver.CurrentActivity, config);
        return await dialog.ShowAsync(message, title, ok, cancel, neutral, defaultPositive).ConfigureAwait(true);
    }

    public async partial ValueTask<int> SelectAsync(string[] items, int selected, string? title, string? cancel)
    {
        using var dialog = new SelectDialog(ActivityResolver.CurrentActivity, config);
        return await dialog.ShowAsync(items, selected, title, cancel).ConfigureAwait(true);
    }

    public async partial ValueTask<PromptResult> PromptAsync(string? defaultValue, string? message, string? title, string ok, string cancel, string? placeHolder, PromptParameter? parameter)
    {
        using var dialog = new PromptDialog(ActivityResolver.CurrentActivity, config);
        return await dialog.ShowAsync(defaultValue, message, title, ok, cancel, placeHolder, parameter ?? new PromptParameter()).ConfigureAwait(true);
    }

    public partial IDisposable Indicator()
    {
        var activity = ActivityResolver.CurrentActivity;
        var size = (int)(deviceDisplay.MainDisplayInfo.Density * config.IndicatorSize);

        using var parameters = new FrameLayout.LayoutParams(size, size);
        parameters.Gravity = GravityFlags.Center;
        using var filter = new Android.Graphics.BlendModeColorFilter(config.IndicatorColor.ToPlatform(), Android.Graphics.BlendMode.SrcAtop!);
        using var progress = new ProgressBar(activity);
        progress.Indeterminate = true;
        progress.LayoutParameters = parameters;
        progress.IndeterminateDrawable!.SetColorFilter(filter);
        using var layout = new FrameLayout(activity);
        layout.AddView(progress);

        using var background = new ColorDrawable(Android.Graphics.Color.Transparent);
        using var builder = new MaterialAlertDialogBuilder(activity);
        builder.SetView(layout)!.SetCancelable(false);
        builder.SetBackground(background);
        var dialog = builder.Create();
        dialog.SetOnKeyListener(IndicatorKeyListener.Instance);
        dialog.Show();

        return new IndicatorDismiss(dialog);
    }

    private sealed class IndicatorKeyListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
    {
        public static IndicatorKeyListener Instance { get; } = new();

        public bool OnKey(IDialogInterface? dialog, Keycode keyCode, KeyEvent? e) => keyCode == Keycode.Escape;
    }

    private sealed class IndicatorDismiss : IDisposable
    {
        private readonly AlertDialog dialog;

        public IndicatorDismiss(AlertDialog dialog)
        {
            this.dialog = dialog;
        }

        public void Dispose()
        {
            dialog.Dismiss();
            dialog.Dispose();
        }
    }

    private abstract class KeyDialog : Java.Lang.Object, IDialogInterfaceOnKeyListener
    {
        private readonly Keycode[] dismissKeys;

        private readonly bool buttonFocus;

        protected Activity Activity { get; }

        protected AlertDialog? AlertDialog { get; private set; }

        protected KeyDialog(Activity activity, DialogConfig config, Keycode[] dismissKeys)
        {
            Activity = activity;
            buttonFocus = config.EnableDialogButtonFocus;
            this.dismissKeys = dismissKeys;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AlertDialog?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected void Show(AlertDialog alertDialog)
        {
            AlertDialog = alertDialog;
            alertDialog.SetOnKeyListener(this);
            alertDialog.Show();
        }

        protected void FocusButton(DialogButtonType type)
        {
            if (buttonFocus && (AlertDialog?.GetButton((int)type) is { } button))
            {
                button.Focusable = true;
                button.FocusableInTouchMode = true;
                button.RequestFocus();
            }
        }

        protected abstract void Cancel();

        public bool OnKey(IDialogInterface? dialog, Keycode keyCode, KeyEvent? e)
        {
            if ((e is null) || !dismissKeys.Contains(e.KeyCode))
            {
                return false;
            }

            if (e.Action == KeyEventActions.Up)
            {
                AlertDialog?.Dismiss();
                Cancel();
            }

            return true;
        }
    }

    private sealed class InformationDialog : KeyDialog
    {
        private readonly TaskCompletionSource result = new();

        public InformationDialog(Activity activity, DialogConfig config)
            : base(activity, config, config.DismissKeys)
        {
        }

        public Task ShowAsync(string message, string? title, string ok)
        {
            using var builder = new MaterialAlertDialogBuilder(Activity);
            Show(builder
                .SetTitle(title)!
                .SetMessage(message)!
                .SetCancelable(false)!
                .SetPositiveButton(ok, (_, _) => result.TrySetResult())
                .Create());
            FocusButton(DialogButtonType.Positive);
            return result.Task;
        }

        protected override void Cancel() => result.TrySetResult();
    }

    private sealed class ConfirmDialog : KeyDialog
    {
        private readonly TaskCompletionSource<bool> result = new();

        public ConfirmDialog(Activity activity, DialogConfig config)
            : base(activity, config, config.DismissKeys)
        {
        }

        public Task<bool> ShowAsync(string message, string? title, string ok, string cancel, bool defaultPositive)
        {
            using var builder = new MaterialAlertDialogBuilder(Activity);
            Show(builder
                .SetTitle(title)!
                .SetMessage(message)!
                .SetCancelable(false)!
                .SetPositiveButton(ok, (_, _) => result.TrySetResult(true))
                .SetNegativeButton(cancel, (_, _) => result.TrySetResult(false))
                .Create());
            FocusButton(defaultPositive ? DialogButtonType.Positive : DialogButtonType.Negative);
            return result.Task;
        }

        protected override void Cancel() => result.TrySetResult(false);
    }

    private sealed class Confirm3Dialog : KeyDialog
    {
        private readonly TaskCompletionSource<Confirm3Result> result = new();

        public Confirm3Dialog(Activity activity, DialogConfig config)
            : base(activity, config, config.DismissKeys)
        {
        }

        public Task<Confirm3Result> ShowAsync(string message, string? title, string ok, string cancel, string neutral, bool defaultPositive)
        {
            using var builder = new MaterialAlertDialogBuilder(Activity);
            Show(builder
                .SetTitle(title)!
                .SetMessage(message)!
                .SetCancelable(false)!
                .SetPositiveButton(ok, (_, _) => result.TrySetResult(Confirm3Result.Positive))
                .SetNegativeButton(cancel, (_, _) => result.TrySetResult(Confirm3Result.Negative))
                .SetNeutralButton(neutral, (_, _) => result.TrySetResult(Confirm3Result.Neutral))
                .Create());
            FocusButton(defaultPositive ? DialogButtonType.Positive : DialogButtonType.Negative);
            return result.Task;
        }

        protected override void Cancel() => result.TrySetResult(Confirm3Result.Negative);
    }

    private sealed class SelectDialog : KeyDialog
    {
        private readonly TaskCompletionSource<int> result = new();

        private readonly Color selectColor;

        public SelectDialog(Activity activity, DialogConfig config)
            : base(activity, config, config.DismissKeys)
        {
            selectColor = config.SelectColor;
        }

        public Task<int> ShowAsync(string[] items, int selected, string? title, string? cancel)
        {
            using var builder = new MaterialAlertDialogBuilder(Activity);
            Show(builder
                .SetTitle(title)!
                .SetItems(items, (_, args) => result.TrySetResult(args.Which))
                .SetCancelable(false)!
                .SetNegativeButton(cancel!, (_, _) => result.TrySetResult(-1))
                .Create());

            if (AlertDialog?.ListView is { } listView)
            {
                listView.Selector = new ColorDrawable(selectColor.ToPlatform());
                if (selected >= 0)
                {
                    listView.SetSelection(selected);
                }

                listView.RequestFocus();
            }

            return result.Task;
        }

        protected override void Cancel() => result.TrySetResult(-1);
    }

    private sealed class PromptDialog : KeyDialog, TextView.IOnEditorActionListener
    {
        private readonly TaskCompletionSource<PromptResult> result = new();

        private readonly bool enterAction;

        private readonly bool selectAll;

        private EditText? input;

        private InputFilterLengthFilter? lengthFilter;

        public PromptDialog(Activity activity, DialogConfig config)
            : base(activity, config, config.DismissKeys.Except(config.IgnorePromptDismissKeys).ToArray())
        {
            enterAction = config.EnablePromptEnterAction;
            selectAll = config.EnablePromptSelectAll;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                input?.Dispose();
                lengthFilter?.Dispose();
            }

            base.Dispose(disposing);
        }

        public Task<PromptResult> ShowAsync(string? defaultValue, string? message, string? title, string ok, string cancel, string? placeHolder, PromptParameter parameter)
        {
            var editText = new EditText(Activity)
            {
                Hint = placeHolder,
                Text = defaultValue,
                InputType = ResolveInputType(parameter),
                ShowSoftInputOnFocus = !HardKeyboard.SuppressSoftInput
            };
            input = editText;

            if (parameter.MaxLength > 0)
            {
                lengthFilter = new InputFilterLengthFilter(parameter.MaxLength);
                editText.SetFilters([.. editText.GetFilters() ?? [], lengthFilter]);
            }

            if (enterAction)
            {
                editText.SetOnEditorActionListener(this);
            }

            if (selectAll)
            {
                editText.SetSelectAllOnFocus(true);
            }

            using var builder = new MaterialAlertDialogBuilder(Activity);
            var alertDialog = builder
                .SetTitle(title)!
                .SetMessage(message)!
                .SetView(editText)!
                .SetCancelable(false)!
                .SetPositiveButton(ok, (_, _) => result.TrySetResult(new PromptResult(true, editText.Text!)))
                .SetNegativeButton(cancel, (_, _) => result.TrySetResult(PromptResult.Cancel))
                .Create();
            alertDialog.Window!.SetSoftInputMode(HardKeyboard.SuppressSoftInput ? SoftInput.StateHidden : SoftInput.StateVisible);
            Show(alertDialog);

            editText.RequestFocus();

            return result.Task;
        }

        private static InputTypes ResolveInputType(PromptParameter parameter)
        {
            var sign = parameter.Sign ? InputTypes.NumberFlagSigned : 0;
            return parameter.PromptType switch
            {
                PromptType.Number => InputTypes.ClassNumber | sign,
                PromptType.Decimal => InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | sign,
                _ => InputTypes.ClassText
            };
        }

        protected override void Cancel() => result.TrySetResult(PromptResult.Cancel);

        public bool OnEditorAction(TextView? v, ImeAction actionId, KeyEvent? e)
        {
            if ((e is not null) && (e.Action == KeyEventActions.Down) && (e.KeyCode == Keycode.Enter))
            {
                var text = v?.Text!;
                AlertDialog?.Dismiss();
                result.TrySetResult(new PromptResult(true, text));
                return true;
            }

            return false;
        }
    }
}
