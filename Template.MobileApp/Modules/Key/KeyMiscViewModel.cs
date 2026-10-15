namespace Template.MobileApp.Modules.Key;

public sealed partial class KeyMiscViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Result { get; set; } = "-";

    public IObserveCommand InformationCommand { get; }
    public IObserveCommand ConfirmCommand { get; }
    public IObserveCommand Confirm3Command { get; }
    public IObserveCommand SelectCommand { get; }
    public IObserveCommand PromptCommand { get; }
    public IObserveCommand NumberCommand { get; }
    public IObserveCommand IndicatorCommand { get; }
    public IObserveCommand LoadingCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public KeyMiscViewModel(
        IDialog dialog,
        IPopupNavigator popupNavigator)
    {
        InformationCommand = MakeAsyncCommand(async () =>
        {
            await dialog.InformationAsync("Information");
            Result = "Information";
        });
        ConfirmCommand = MakeAsyncCommand(async () =>
        {
            var result = await dialog.ConfirmAsync("Confirm");
            Result = $"Confirm={result}";
        });
        Confirm3Command = MakeAsyncCommand(async () =>
        {
            var result = await dialog.Confirm3Async("Confirm3");
            Result = $"Confirm3={result}";
        });
        SelectCommand = MakeAsyncCommand(async () =>
        {
            var result = await dialog.SelectAsync(["Item-1", "Item-2", "Item-3"], cancel: "Cancel");
            Result = $"Select={result}";
        });
        PromptCommand = MakeAsyncCommand(async () =>
        {
            var result = await dialog.PromptAsync(defaultValue: "123", parameter: new PromptParameter { PromptType = PromptType.Number, MaxLength = 5 });
            Result = result.Accepted ? $"Prompt={result.Text}" : "Prompt=Cancel";
        });
        NumberCommand = MakeAsyncCommand(async () =>
        {
            var result = await popupNavigator.InputNumberAsync("Number", "0", 5);
            Result = $"Number={result ?? "Cancel"}";
        });
        IndicatorCommand = MakeAsyncCommand(async () =>
        {
            using (dialog.Indicator())
            {
                await Task.Delay(3000);
            }

            Result = "Indicator";
        });
        LoadingCommand = MakeAsyncCommand(async () =>
        {
            using (dialog.Loading("Loading"))
            {
                await Task.Delay(3000);
            }

            Result = "Loading";
        });
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
