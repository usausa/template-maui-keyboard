namespace Template.MobileApp.Behaviors;

#pragma warning disable CA1724
public static class Focus
{
    //--------------------------------------------------------------------------------
    // SuppressDefaultFocus
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty SuppressDefaultFocusProperty = BindableProperty.CreateAttached(
        "SuppressDefaultFocus",
        typeof(bool),
        typeof(Focus),
        false);

    public static bool GetSuppressDefaultFocus(BindableObject bindable) => (bool)bindable.GetValue(SuppressDefaultFocusProperty);

    public static void SetSuppressDefaultFocus(BindableObject bindable, bool value) => bindable.SetValue(SuppressDefaultFocusProperty, value);
}
#pragma warning restore CA1724
