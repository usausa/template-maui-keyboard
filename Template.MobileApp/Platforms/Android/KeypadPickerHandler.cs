#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Template.MobileApp;

using Android.App;
using Android.Content;
using Android.Views;

using Microsoft.Maui.Handlers;

// 日付・時刻のダイアログは Esc 1 回でキャンセルして閉じる(Android の既定では 1 回目はダイアログの中のフォーカスを外す)
public sealed class KeypadDatePickerHandler : DatePickerHandler
{
    protected override DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
    {
        var dialog = base.CreateDatePickerDialog(year, month, day);
        dialog.SetOnKeyListener(DialogCancelKeyListener.Instance);
        return dialog;
    }
}

public sealed class KeypadTimePickerHandler : TimePickerHandler
{
    protected override TimePickerDialog CreateTimePickerDialog(int hour, int minute)
    {
        var dialog = base.CreateTimePickerDialog(hour, minute);
        dialog.SetOnKeyListener(DialogCancelKeyListener.Instance);
        return dialog;
    }
}

internal sealed class DialogCancelKeyListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
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
