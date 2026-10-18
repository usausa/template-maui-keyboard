namespace Template.MobileApp.Helpers;

using Android.Views;

public static partial class ElementHelper
{
    private static partial bool IsPlatformFocusable(VisualElement visual) =>
        (visual.Handler?.PlatformView is View view) && view.Focusable;

    private static partial bool IsPlatformFocusWithin(VisualElement visual) =>
        (visual.Handler?.PlatformView is View view) && view.HasFocus;

    public static partial bool IsInTouchMode(VisualElement element) =>
        (element.Handler?.PlatformView is View view) && view.IsInTouchMode;

    private static partial void PlatformRequestFocus(VisualElement element)
    {
        if (element.Handler?.PlatformView is View view)
        {
            view.RequestFocusFromTouch();
        }
    }
}
