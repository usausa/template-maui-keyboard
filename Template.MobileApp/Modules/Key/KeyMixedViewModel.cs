namespace Template.MobileApp.Modules.Key;

public sealed partial class KeyMixedViewModel : AppViewModelBase
{
    public IReadOnlyList<string> Items { get; } = Enumerable.Range(1, 5).Select(static x => $"Item-{x}").ToArray();

    [ObservableProperty]
    public partial double StepperValue { get; set; }

    [ObservableProperty]
    public partial int ButtonCount { get; set; }

    public IObserveCommand ButtonCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public KeyMixedViewModel()
    {
        ButtonCommand = MakeDelegateCommand(() => ButtonCount++);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
