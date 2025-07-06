namespace Template.MobileApp;

using BunnyTail.EmbeddedBuildProperty;

internal static partial class Variants
{
    [BuildProperty]
    public static partial string Flavor { get; }
}
