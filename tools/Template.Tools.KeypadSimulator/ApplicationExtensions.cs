namespace Template.Tools.KeypadSimulator;

using BunnyTail.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Template.Tools.KeypadSimulator.Services;
using Template.Tools.KeypadSimulator.Settings;
using Template.Tools.KeypadSimulator.Views;

public static partial class ApplicationExtensions
{
    //--------------------------------------------------------------------------------
    // Container
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureContainer(this IHostApplicationBuilder builder)
    {
        builder.ConfigureContainer(new GeneratedServiceProviderFactory(static options => options.TrackTransientDisposables = false));

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Logging
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Components
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureComponents(this IHostApplicationBuilder builder)
    {
        // Setting
        builder.Services.AddOptions<SimulatorSettings>().BindConfiguration("Simulator");
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<SimulatorSettings>>().Value);
        builder.Services.AddSingleton<UserSettings>();
        builder.Services.AddSingleton<KeypadKeyMap>();

        // Service
        builder.Services.AddServices();

        // Window
        builder.Services.AddSingleton<IWindowManager, WindowManager>();
        builder.Services.AddSingleton<MainWindow>();
        // ViewModel
        builder.Services.AddViewModels();

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Service
    //--------------------------------------------------------------------------------

    [ComponentRegistration(Lifetime.Singleton, "Service$")]
    public static partial IServiceCollection AddServices(this IServiceCollection services);

    //--------------------------------------------------------------------------------
    // ViewModel
    //--------------------------------------------------------------------------------

    [ComponentRegistration(Lifetime.Transient, "ViewModel$")]
    public static partial IServiceCollection AddViewModels(this IServiceCollection services);
}
