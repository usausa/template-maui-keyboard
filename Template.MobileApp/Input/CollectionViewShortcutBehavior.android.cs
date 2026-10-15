namespace Template.MobileApp.Input;

using Android.Views;

public sealed partial class CollectionViewShortcutBehavior
{
    private static partial void UpdatePlatformFocus(CollectionView view)
    {
        if (view.Handler?.PlatformView is ViewGroup group)
        {
            group.DescendantFocusability = DescendantFocusability.BlockDescendants;
            group.Focusable = true;
        }
    }
}
