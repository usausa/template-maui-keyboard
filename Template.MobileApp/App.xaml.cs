namespace Template.MobileApp;

using Microsoft.Extensions.DependencyInjection;

using Template.MobileApp.Diagnostics;

#pragma warning disable CA1724
public sealed partial class App
{
    private readonly IServiceProvider serviceProvider;

    private readonly ILogger<App> log;

    public App(IServiceProvider serviceProvider, ILogger<App> log)
    {
        this.serviceProvider = serviceProvider;
        this.log = log;

        // Light theme based application
        Current!.UserAppTheme = AppTheme.Light;

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(serviceProvider.GetRequiredService<MainPage>());
    }

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnStart()
    {
        // Report previous exception
        await CrashReport.ShowReport();

        // Start
        log.InfoApplicationStart(typeof(App).Assembly.GetName().Version, Environment.Version);

        // Completed
        serviceProvider.GetRequiredService<StartupState>().NotifyCompleted();
    }
}
#pragma warning restore CA1724
