namespace Template.MobileApp.Components;

using IProgress = MauiComponents.IProgress;

// MauiComponents の DialogImplementation に、物理キーで閉じた時の扱いを足した版(メッセージのダイアログ以外は元の実装に任せる)
public sealed partial class KeypadDialog : IDialog
{
    private readonly DialogConfig config;

    private readonly IDeviceDisplay deviceDisplay;

    private readonly DialogImplementation implementation;

    public KeypadDialog(
        DialogConfig config,
        IDeviceDisplay deviceDisplay)
    {
        this.config = config;
        this.deviceDisplay = deviceDisplay;
        implementation = new DialogImplementation(config, deviceDisplay);
    }

    public partial ValueTask InformationAsync(string message, string? title, string ok);

    public partial ValueTask<bool> ConfirmAsync(string message, string? title, string ok, string cancel, bool defaultPositive);

    public partial ValueTask<Confirm3Result> Confirm3Async(string message, string? title, string ok, string cancel, string neutral, bool defaultPositive);

    public partial ValueTask<int> SelectAsync(string[] items, int selected, string? title, string? cancel);

    public partial ValueTask<PromptResult> PromptAsync(string? defaultValue, string? message, string? title, string ok, string cancel, string? placeHolder, PromptParameter? parameter);

    public partial IDisposable Indicator();

    public IDisposable Lock() => implementation.Lock();

    public ILoading Loading(string text) => implementation.Loading(text);

    public IProgress Progress() => implementation.Progress();

    public void Snackbar(string message, int duration, Color? color, Color? textColor) => implementation.Snackbar(message, duration, color, textColor);

    public ValueTask Toast(string text, bool longDuration, double textSize) => implementation.Toast(text, longDuration, textSize);
}
