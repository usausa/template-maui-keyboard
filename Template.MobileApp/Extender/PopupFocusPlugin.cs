namespace Template.MobileApp.Extender;

using CommunityToolkit.Maui.Views;

using Template.MobileApp.Input;

public sealed class PopupFocusPlugin : IPopupPlugin
{
    public void Extend(ContentView view)
    {
        view.Content?.Behaviors.Add(new InputPopupBehavior());
        if (view is Popup popup)
        {
            popup.Opened += (_, _) =>
            {
                Application.Current?.Dispatcher.Dispatch(() => view.Content?.SetDefaultFocus());
            };
        }
    }
}
