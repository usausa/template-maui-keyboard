namespace Template.MobileApp.Modules.Key;

public sealed partial class KeyControlViewModel : AppViewModelBase
{
    // Property

    [ObservableProperty]
    public partial bool CheckA { get; set; }

    [ObservableProperty]
    public partial bool CheckB { get; set; }

    [ObservableProperty]
    public partial bool SwitchOn { get; set; }

    [ObservableProperty]
    public partial string RadioValue { get; set; } = "A";

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
