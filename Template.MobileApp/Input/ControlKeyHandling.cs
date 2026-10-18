namespace Template.MobileApp.Input;

public static partial class ControlKeyHandling
{
    public static MauiAppBuilder ConfigureControlKeyHandling(this MauiAppBuilder builder)
    {
        ConfigurePlatform(builder);
        return builder;
    }

    private static partial void ConfigurePlatform(MauiAppBuilder builder);
}
