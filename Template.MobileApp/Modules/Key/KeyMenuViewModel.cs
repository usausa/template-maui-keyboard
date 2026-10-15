namespace Template.MobileApp.Modules.Key;

public sealed class KeyMenuViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    public IObserveCommand ForwardCommand { get; }

    public KeyMenuViewModel(
        IDialog dialog)
    {
        this.dialog = dialog;

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    // ルートの画面の戻るは、確かめてからアプリを裏に回す(既定のボタンは Cancel)
    protected override async Task OnNotifyBackAsync()
    {
        if (await dialog.ConfirmAsync("Exit?"))
        {
            AndroidHelper.MoveTaskToBack();
        }
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
