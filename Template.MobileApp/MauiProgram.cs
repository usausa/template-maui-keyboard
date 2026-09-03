namespace Template.MobileApp;

using BunnyTail.DependencyInjection;

using CommunityToolkit.Maui;

using Fonts;

using Microsoft.Maui.LifecycleEvents;

#if false
using Plugin.Maui.DebugRainbows;
#endif

using SkiaSharp.Views.Maui.Controls.Hosting;

using Syncfusion.Maui.Toolkit.Hosting;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Components;
using Template.MobileApp.Extender;
using Template.MobileApp.Helpers;
using Template.MobileApp.Modules;

public static partial class MauiProgram
{
    private const string ModulesNamespace = "Template.MobileApp.Modules";

    public static MauiApp CreateMauiApp() =>
        MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureDebug()
            .ConfigureFonts(ConfigureFonts)
            .ConfigureLifecycleEvents(ConfigureLifecycleEvents)
            .ConfigureEssentials(ConfigureEssentials)
            .ConfigureLogging()
            .ConfigureGlobalSettings()
            .ConfigureSyncfusionToolkit()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit(ConfigureMauiCommunityToolkit)
            .UseMauiServices()
            .UseMauiComponents()
            .UseCommunityToolkitServices()
            .UseCustomView()
            .ConfigureContainer()
            .Build();

    // ------------------------------------------------------------
    // Debug
    // ------------------------------------------------------------

    private static MauiAppBuilder ConfigureDebug(this MauiAppBuilder builder)
    {
#if DEBUG
#if false
        builder
            .UseDebugRainbows(new DebugRainbowsOptions
            {
                ShowRainbows = true,
                ShowGrid = true,
                HorizontalItemSize = 20,
                VerticalItemSize = 20,
                MajorGridLineInterval = 4,
                MajorGridLines = new GridLineOptions { Color = Color.FromRgb(255, 0, 0), Opacity = 0.5, Width = 3 },
                MinorGridLines = new GridLineOptions { Color = Color.FromRgb(255, 0, 0), Opacity = 0.25, Width = 1 },
                GridOrigin = DebugGridOrigin.TopLeft
            });
#endif
#endif
        return builder;
    }

    // ------------------------------------------------------------
    // Logging
    // ------------------------------------------------------------

    private static MauiAppBuilder ConfigureLogging(this MauiAppBuilder builder)
    {
        // Debug
#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Android
#if ANDROID
        builder.Logging.AddAndroidLogger(static options => options.ShortCategory = true);
#endif
        // File
        builder.Logging.AddFileLogger(static options =>
            {
#if ANDROID
                options.Directory = Path.Combine(AndroidHelper.GetExternalFilesDir(), "log");
#endif
                options.RetainDays = 7;
            })
            .AddFilter(typeof(MauiProgram).Namespace, LogLevel.Debug);

        return builder;
    }

    // ------------------------------------------------------------
    // Application
    // ------------------------------------------------------------

    // ReSharper disable UnusedParameter.Local
    private static void ConfigureLifecycleEvents(ILifecycleBuilder effects)
    {
    }
    // ReSharper restore UnusedParameter.Local

    // ReSharper disable UnusedParameter.Local
    private static void ConfigureEssentials(IEssentialsBuilder config)
    {
    }
    // ReSharper restore UnusedParameter.Local

    private static void ConfigureMauiCommunityToolkit(Options options)
    {
        options.SetPopupDefaults(new DefaultPopupSettings
        {
            CanBeDismissedByTappingOutsideOfPopup = false,
            Padding = 0
        });
        options.SetPopupOptionsDefaults(new DefaultPopupOptionsSettings
        {
            CanBeDismissedByTappingOutsideOfPopup = false,
            Shadow = null,
            Shape = null
        });
    }

    private static MauiAppBuilder ConfigureGlobalSettings(this MauiAppBuilder builder)
    {
        // TODO App center alternative

        // Crash dump
        CrashReport.Start();

        return builder;
    }

    private static MauiAppBuilder UseCustomView(this MauiAppBuilder builder)
    {
        // Behaviors
        builder.ConfigureCustomBehaviors(static options =>
        {
            options.HandleEnterKey = true;
            options.DisableShowSoftInputOnFocus = true;
        });

        return builder;
    }

    // ------------------------------------------------------------
    // Design
    // ------------------------------------------------------------

    private static void ConfigureFonts(IFontCollection fonts)
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
        fonts.AddFont("MaterialIcons-Regular.ttf", MaterialIcons.FontFamily);
        fonts.AddFont("851Gkktt_005.ttf", "Gkktt");
        fonts.AddFont("DSEG7Classic-Regular.ttf", "DSEG7");
    }

    private static void ConfigureDialogDesign(DialogConfig config)
    {
        var resources = Application.Current!.Resources;
        config.IndicatorColor = resources.FindResource<Color>("BlueAccent2");
        config.LoadingMessageFontSize = 28;
        config.ProgressCircleColor1 = resources.FindResource<Color>("BlueAccent2");
        config.ProgressCircleColor2 = resources.FindResource<Color>("GrayLighten2");

        // Avoiding conflicts with progress
        config.LockBackgroundColor = Colors.Transparent;
        config.LoadingBackgroundColor = Colors.Transparent;
        config.ProgressBackgroundColor = Colors.Transparent;
    }

    // ------------------------------------------------------------
    // Components
    // ------------------------------------------------------------

    private static MauiAppBuilder ConfigureContainer(this MauiAppBuilder builder)
    {
        builder.ConfigureContainer(
            new GeneratedServiceProviderFactory(static options => options.TrackTransientDisposables = false),
            ConfigureContainer);
        return builder;
    }

    private static void ConfigureContainer(IServiceCollection services)
    {
        // View & ViewModel
        services.AddTransient<MainPage>();
        services.AddTransient<MainPageViewModel>();
        services.AddViews();
        services.AddViewModels();

        // MauiComponents
        services.AddComponentsDialog(static c =>
        {
            ConfigureDialogDesign(c);
#if DEVICE_HAS_KEYPAD
            c.DismissKeys = new[] { Keycode.Escape, Keycode.Del };
            c.IgnorePromptDismissKeys = new[] { Keycode.Del };
            c.EnableDialogButtonFocus = true;
#endif
            c.EnablePromptEnterAction = true;
            c.EnablePromptSelectAll = true;
        });
        services.AddComponentsPopup(static c => c.AutoRegister(DialogSource()));
        services.AddComponentsPopupPlugin<PopupFocusPlugin>();
        services.AddComponentsScreen();
        services.AddComponentsLocation();
        services.AddComponentsSpeech();
        services.AddCommunication();

        // Messenger
        services.AddSingleton<IReactiveMessenger>(ReactiveMessenger.Default);

        // Navigator
        services.AddNavigator(static (_, config) =>
        {
            config.UseMauiNavigationProvider();
            config.AddPlugin<NavigationFocusPlugin>();
            config.UseIdViewMapper(static m => m.AutoRegister(ViewSource()));
        });

        // Components
        services.AddSingleton<IStorageManager, StorageManager>();

        // Resource
        services.AddSingleton<ResourceDictionary>(static _ => Application.Current!.Resources);

        // State
        services.AddSingleton<DeviceState>();

        // Startup
        services.AddSingleton<IMauiInitializeService, ApplicationInitializer>();
    }

    // ------------------------------------------------------------
    // View & ViewModel
    // ------------------------------------------------------------

    // ReSharper disable UnusedMethodReturnValue.Local
    [ComponentRegistration(Lifetime.Transient, "View$", Namespace = ModulesNamespace)]
    private static partial IServiceCollection AddViews(this IServiceCollection services);

    [ComponentRegistration(Lifetime.Transient, "ViewModel$", Namespace = ModulesNamespace)]
    private static partial IServiceCollection AddViewModels(this IServiceCollection services);
    // ReSharper restore UnusedMethodReturnValue.Local

    // ------------------------------------------------------------
    // Navigation
    // ------------------------------------------------------------

    [ViewSource]
    public static partial IEnumerable<KeyValuePair<ViewId, Type>> ViewSource();

    [PopupSource]
    public static partial IEnumerable<KeyValuePair<DialogId, Type>> DialogSource();
}
