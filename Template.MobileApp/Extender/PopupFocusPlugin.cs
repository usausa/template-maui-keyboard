namespace Template.MobileApp.Extender;

using CommunityToolkit.Maui.Views;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Helpers;
using Template.MobileApp.Input;

public sealed class PopupFocusPlugin : IPopupPlugin
{
    private readonly Dictionary<ContentView, VisualElement> focusBackup = [];

    public void Extend(ContentView view)
    {
        if ((view is Popup popup) && !Focus.GetSuppressDefaultFocus(view))
        {
            popup.Opened += (_, _) =>
            {
                Application.Current?.Dispatcher.Dispatch(() => view.Content?.SetDefaultFocus());
            };
        }
    }

    public void OnOpening(ContentView view)
    {
        var focused = InputManager.Default.FindFocused();
        if (focused is not null)
        {
            focusBackup[view] = focused;
        }
    }

    // キーで閉じた時は開く前の要素へ戻す(タッチで閉じた時は IDialog と同じく戻さない)
    public void OnClosed(ContentView view)
    {
        if (focusBackup.Remove(view, out var focused) && !ElementHelper.IsInTouchMode(focused))
        {
            Application.Current?.Dispatcher.Dispatch(() => ElementHelper.RequestFocus(focused));
        }
    }
}
