namespace Template.MobileApp;

using CommunityToolkit.Maui.Core;

using Template.MobileApp.Modules;
using Template.MobileApp.Shell;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed class MainPageViewModel : ExtendViewModelBase, IShellControl, IAppLifecycle
{
    private readonly IScreen screen;

    private bool destroying;

    private IDisposable? navigatingBusy;

    public StartupState Startup { get; }

    public INavigator Navigator { get; }

    public NotificationValue<string> Title { get; } = new(string.Empty);

    public NotificationValue<bool> HeaderVisible { get; } = new();

    public NotificationValue<bool> FunctionVisible { get; } = new();

    public NotificationValue<Color?> StatusBarColor { get; } = new();

    public NotificationValue<StatusBarStyle> StatusBarStyle { get; } = new(CommunityToolkit.Maui.Core.StatusBarStyle.Default);

    public IReadOnlyList<FunctionState> Functions { get; } = [new(), new(), new(), new()];

    public FunctionState Function1 => Functions[0];
    public FunctionState Function2 => Functions[1];
    public FunctionState Function3 => Functions[2];
    public FunctionState Function4 => Functions[3];

    public IObserveCommand Function1Command { get; }
    public IObserveCommand Function2Command { get; }
    public IObserveCommand Function3Command { get; }
    public IObserveCommand Function4Command { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MainPageViewModel(
        ILogger<MainPageViewModel> log,
        StartupState startup,
        INavigator navigator,
        IScreen screen,
        IDialog dialog)
    {
        Startup = startup;
        Navigator = navigator;
        this.screen = screen;

        Function1Command = CreateFunctionCommand(Functions[0], ShellEvent.Function1);
        Function2Command = CreateFunctionCommand(Functions[1], ShellEvent.Function2);
        Function3Command = CreateFunctionCommand(Functions[2], ShellEvent.Function3);
        Function4Command = CreateFunctionCommand(Functions[3], ShellEvent.Function4);

        // Busy while navigating
        Disposables.Add(Observable.FromEventPattern<EventArgs>(h => Navigator.ExecutingChanged += h, h => Navigator.ExecutingChanged -= h)
            .Subscribe(_ => UpdateNavigatingBusy()));

        // Screen lock detection
        // ReSharper disable AsyncVoidLambda
        Disposables.Add(screen.StateChangedAsObservable().ObserveOnCurrentContext().Subscribe(async x =>
        {
            log.DebugScreenStateChanged(x.ScreenOn);
            if (x.ScreenOn)
            {
                await dialog.Toast("Screen on", true);
            }
        }));
        // ReSharper restore AsyncVoidLambda
    }

    private IObserveCommand CreateFunctionCommand(FunctionState function, ShellEvent shellEvent)
    {
        var command = MakeAsyncCommand(() => Navigator.NotifyAsync(shellEvent), () => function.Enabled.Value);
        return Observe(function.Enabled.AsObservable(nameof(NotificationValue<>.Value)), command);
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidMethod
    public async void OnCreated()
    {
        screen.EnableDetectScreenState(true);

        await Startup.Completed;

        // Guard for the case where the Activity is recreated while initialization is still in progress
        if (destroying)
        {
            return;
        }

        Navigator.Exit();
        await Navigator.ForwardAsync(ViewId.KeyMenu);
    }

    public void OnActivated()
    {
    }

    public void OnDeactivated()
    {
    }

    public void OnStopped()
    {
    }

    public void OnResumed()
    {
    }

    public void OnDestroying()
    {
        destroying = true;
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    private void UpdateNavigatingBusy()
    {
        if (Navigator.Executing)
        {
            navigatingBusy ??= BusyState.Begin();
        }
        else
        {
            navigatingBusy?.Dispose();
            navigatingBusy = null;
        }
    }
}
