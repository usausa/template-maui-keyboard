namespace Template.MobileApp.Extender;

using CommunityToolkit.Maui.Views;

using Template.MobileApp.Input;

public sealed class PopupInputPlugin : IPopupPlugin
{
    public void Extend(ContentView view)
    {
        if (view is Popup popup)
        {
            var behavior = new InputBehavior();
            behavior.SetBinding(
                InputBehavior.IsBusyProperty,
                static (ExtendViewModelBase viewModel) => viewModel.BusyState.IsBusy);
            popup.Behaviors.Add(behavior);
        }
    }

    public void OnOpening(ContentView view)
    {
    }

    public void OnClosed(ContentView view)
    {
    }
}
