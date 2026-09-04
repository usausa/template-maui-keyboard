namespace Template.MobileApp;

using Microsoft.Extensions.DependencyInjection;

using Template.MobileApp.Helpers;
using Template.MobileApp.Modules;

#pragma warning disable CA1724
public sealed partial class App
{
    private readonly IServiceProvider serviceProvider;

    private readonly ILogger<App> log;

    private bool windowCreated;

    public App(IServiceProvider serviceProvider, ILogger<App> log)
    {
        this.serviceProvider = serviceProvider;
        this.log = log;

        // Light theme based application
        Current!.UserAppTheme = AppTheme.Light;

        InitializeComponent();

        // Start
        log.InfoApplicationStart(typeof(App).Assembly.GetName().Version, Environment.Version);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(serviceProvider.GetRequiredService<MainPage>());

        if (windowCreated)
        {
            window.Created += OnWindowRecreated;
        }

        windowCreated = true;

        return window;
    }

    private void OnWindowRecreated(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            window.Created -= OnWindowRecreated;
        }

        RestoreInitialViewAsync().ContinueWith(
            t => log.WarnWindowRecreateError(t.Exception!),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    private async Task RestoreInitialViewAsync()
    {
        var navigator = serviceProvider.GetRequiredService<INavigator>();

        if (navigator.CurrentViewId is null)
        {
            return;
        }

        log.InfoWindowRecreated();

        navigator.Exit();

        await navigator.ForwardAsync(ViewId.KeyMenu);
    }

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnStart()
    {
        // Report previous exception
        await CrashReport.ShowReport();

        // Navigate
        var navigator = serviceProvider.GetRequiredService<INavigator>();
        await navigator.ForwardAsync(ViewId.KeyMenu);
    }
}
#pragma warning restore CA1724
