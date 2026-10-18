namespace Template.MobileApp.Modules.Key;

public sealed partial class KeyListViewModel : AppViewModelBase
{
    // Property

    [ObservableProperty]
    public partial string Result { get; set; } = "-";

    public IReadOnlyList<string> Items { get; } = Enumerable.Range(1, 20).Select(static x => $"Item-{x}").ToArray();

    // Command

    public IObserveCommand SelectCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public KeyListViewModel()
    {
        SelectCommand = MakeDelegateCommand<string>(x => Result = x);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
