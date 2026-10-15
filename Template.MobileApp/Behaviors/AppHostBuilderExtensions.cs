namespace Template.MobileApp.Behaviors;

internal static class AppHostBuilderExtensions
{
    public static MauiAppBuilder ConfigureCustomBehaviors(this MauiAppBuilder builder)
    {
        return builder.ConfigureCustomBehaviors(static _ => { });
    }

    public static MauiAppBuilder ConfigureCustomBehaviors(this MauiAppBuilder builder, Action<BehaviorOptions> configure)
    {
        var options = new BehaviorOptions();
        configure(options);

        Border.UseCustomMapper(options);
        Scroll.UseCustomMapper(options);
        Focus.UseCustomMapper(options);

        LabelOption.UseCustomMapper(options);
        ButtonOption.UseCustomMapper(options);
        EntryOption.UseCustomMapper(options);
        SliderOption.UseCustomMapper(options);
        StepperOption.UseCustomMapper(options);

        return builder;
    }
}
