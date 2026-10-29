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

    //--------------------------------------------------------------------------------
    // LineHeight
    //--------------------------------------------------------------------------------

    // Line height in dp (0: not fixed). Keeps the same line height for CJK and Latin text to align other elements to the lines
    // Named differently from Label.LineHeight (ratio), because the same name is mixed in the MAUI mapper and skipped for FormattedText
    public static readonly BindableProperty FixedLineHeightProperty = BindableProperty.CreateAttached(
        "FixedLineHeight",
        typeof(double),
        typeof(LabelOption),
        0d);

    public static double GetFixedLineHeight(BindableObject bindable) => (double)bindable.GetValue(FixedLineHeightProperty);

    public static void SetFixedLineHeight(BindableObject bindable, double value) => bindable.SetValue(FixedLineHeightProperty, value);
}
