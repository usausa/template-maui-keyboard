namespace Template.MobileApp;

using Microsoft.Extensions.DependencyInjection;

using Template.MobileApp.Helpers;

#pragma warning disable CA1724
public sealed partial class App
{
    private readonly IServiceProvider serviceProvider;

    public App(IServiceProvider serviceProvider, ILogger<App> log)
    {
        this.serviceProvider = serviceProvider;

        // Light theme based application
        Current!.UserAppTheme = AppTheme.Light;

        InitializeComponent();

        // Start
        log.InfoApplicationStart(typeof(App).Assembly.GetName().Version, Environment.Version);
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

        // Completed
        serviceProvider.GetRequiredService<StartupState>().NotifyCompleted();
    }
}
#pragma warning restore CA1724
