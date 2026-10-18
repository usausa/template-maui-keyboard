namespace Template.MobileApp.Modules.Key;

public sealed class KeyMenuViewModel : AppViewModelBase
{
    // Field

    private readonly IDialog dialog;

    // Command

    public IObserveCommand ForwardCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public KeyMenuViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override async Task OnNotifyBackAsync()
    {
        if (await dialog.ConfirmAsync("Exit?"))
        {
            AndroidHelper.MoveTaskToBack();
        }
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
