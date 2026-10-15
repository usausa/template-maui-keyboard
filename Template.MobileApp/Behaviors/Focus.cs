namespace Template.MobileApp.Behaviors;

public static partial class Focus
{
    public static partial void UseCustomMapper(BehaviorOptions options);

    public static readonly BindableProperty SuppressDefaultFocusProperty = BindableProperty.CreateAttached(
        "SuppressDefaultFocus",
        typeof(bool),
        typeof(Focus),
        false);

    public static bool GetSuppressDefaultFocus(BindableObject bindable) => (bool)bindable.GetValue(SuppressDefaultFocusProperty);

    public static void SetSuppressDefaultFocus(BindableObject bindable, bool value) => bindable.SetValue(SuppressDefaultFocusProperty, value);

    // ReSharper disable InconsistentNaming
    public static readonly BindableProperty DefaultProperty = BindableProperty.CreateAttached(
        "Default",
        typeof(bool),
        typeof(Focus),
        false);
    // ReSharper restore InconsistentNaming

    public static bool GetDefault(BindableObject bindable) => (bool)bindable.GetValue(DefaultProperty);

    public static void SetDefault(BindableObject bindable, bool value) => bindable.SetValue(DefaultProperty, value);

    // 上下左右のフォーカスの移動で飛ばす
    public static readonly BindableProperty SkipProperty = BindableProperty.CreateAttached(
        "Skip",
        typeof(bool),
        typeof(Focus),
        false);

    public static bool GetSkip(BindableObject bindable) => (bool)bindable.GetValue(SkipProperty);

    public static void SetSkip(BindableObject bindable, bool value) => bindable.SetValue(SkipProperty, value);

    // 上下左右のフォーカスの移動で、端から反対の端へ折り返す(画面やポップアップに付ける)
    public static readonly BindableProperty WrapProperty = BindableProperty.CreateAttached(
        "Wrap",
        typeof(bool),
        typeof(Focus),
        false);

    public static bool GetWrap(BindableObject bindable) => (bool)bindable.GetValue(WrapProperty);

    public static void SetWrap(BindableObject bindable, bool value) => bindable.SetValue(WrapProperty, value);

    // 上下左右のフォーカスの移動の範囲
    public static readonly BindableProperty ScopeProperty = BindableProperty.CreateAttached(
        "Scope",
        typeof(bool),
        typeof(Focus),
        false);

    public static bool GetScope(BindableObject bindable) => (bool)bindable.GetValue(ScopeProperty);

    public static void SetScope(BindableObject bindable, bool value) => bindable.SetValue(ScopeProperty, value);

    // フォーカスのある間に Android の既定の色(灰色)を重ねるか(背景の色でフォーカスを示すコントロールは False にする)
    public static readonly BindableProperty DefaultHighlightProperty = BindableProperty.CreateAttached(
        "DefaultHighlight",
        typeof(bool),
        typeof(Focus),
        true);

    public static bool GetDefaultHighlight(BindableObject bindable) => (bool)bindable.GetValue(DefaultHighlightProperty);

    public static void SetDefaultHighlight(BindableObject bindable, bool value) => bindable.SetValue(DefaultHighlightProperty, value);
}
