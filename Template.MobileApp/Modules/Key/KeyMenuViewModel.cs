namespace Template.MobileApp.Modules.Key;

public sealed class KeyMenuViewModel : AppViewModelBase
{
    public IObserveCommand ForwardCommand { get; }

    public KeyMenuViewModel()
    {
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }

    protected override Task OnNotifyBackAsync()
    {
        AndroidHelper.MoveTaskToBack();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
