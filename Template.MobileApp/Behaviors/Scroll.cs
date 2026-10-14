namespace Template.MobileApp.Behaviors;

public static partial class Scroll
{
    // ------------------------------------------------------------------ DisableOverScroll

    public static readonly BindableProperty DisableOverScrollProperty = BindableProperty.CreateAttached(
        "DisableOverScroll",
        typeof(bool),
        typeof(Scroll),
        true);

    public static bool GetDisableOverScroll(BindableObject bindable) => (bool)bindable.GetValue(DisableOverScrollProperty);

    public static void SetDisableOverScroll(BindableObject bindable, bool value) => bindable.SetValue(DisableOverScrollProperty, value);

    public static partial void UseCustomMapper(BehaviorOptions options);
}
