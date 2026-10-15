namespace Template.MobileApp.Behaviors;

using Android.Views;

using Microsoft.Maui.Handlers;

public static partial class Focus
{
    public static partial void UseCustomMapper(BehaviorOptions options)
    {
        if (options.FocusHighlight)
        {
            ViewHandler.ViewMapper.AppendToMapping(DefaultHighlightProperty.PropertyName, UpdateDefaultHighlight);
        }
    }

    // 指定の無いコントロールは Android の既定のまま(入力欄はもともと重ねない)
    private static void UpdateDefaultHighlight(IViewHandler handler, IView view)
    {
        if ((view is BindableObject bindable) && bindable.IsSet(DefaultHighlightProperty) && (handler.PlatformView is View platformView))
        {
            platformView.DefaultFocusHighlightEnabled = GetDefaultHighlight(bindable);
        }
    }
}
