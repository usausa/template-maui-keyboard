namespace Template.MobileApp.Behaviors;

public static partial class LabelOption
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    //--------------------------------------------------------------------------------
    // Size
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty AutoSizeProperty = BindableProperty.CreateAttached(
        "AutoSize",
        typeof(bool),
        typeof(LabelOption),
        false);

    public static bool GetAutoSize(BindableObject bindable) => (bool)bindable.GetValue(AutoSizeProperty);

    public static void SetAutoSize(BindableObject bindable, bool value) => bindable.SetValue(AutoSizeProperty, value);

    public static readonly BindableProperty MaxSizeProperty = BindableProperty.CreateAttached(
        "MaxSize",
        typeof(double),
        typeof(LabelOption),
        144d);

    public static double GetMaxSize(BindableObject bindable) => (double)bindable.GetValue(MaxSizeProperty);

    public static void SetMaxSize(BindableObject bindable, double value) => bindable.SetValue(MaxSizeProperty, value);
}
