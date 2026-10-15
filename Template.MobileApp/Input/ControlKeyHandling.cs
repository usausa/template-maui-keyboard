namespace Template.MobileApp.Input;

// 物理キーで操作するためのコントロールの扱い(Slider・Stepper の左右、日付・時刻のダイアログの Esc、Focus.DefaultHighlight)
public static partial class ControlKeyHandling
{
    public static MauiAppBuilder ConfigureControlKeyHandling(this MauiAppBuilder builder)
    {
        ConfigurePlatform(builder);
        return builder;
    }

    private static partial void ConfigurePlatform(MauiAppBuilder builder);
}
