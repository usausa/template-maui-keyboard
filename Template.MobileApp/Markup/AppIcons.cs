namespace Template.MobileApp.Markup;

using Fonts;

public static class AppIcons
{
    private const double FunctionSize = 36d;

    public static readonly FontImageSource Close = Create(MaterialIcons.Close);

    public static readonly FontImageSource Check = Create(MaterialIcons.Check);

    private static FontImageSource Create(string glyph) =>
        new()
        {
            FontFamily = MaterialIcons.FontFamily,
            Glyph = glyph,
            Size = FunctionSize,
            Color = Colors.White
        };
}
