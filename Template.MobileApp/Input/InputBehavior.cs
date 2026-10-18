namespace Template.MobileApp.Input;

using CommunityToolkit.Maui.Views;

using Smart.Maui.Interactivity;

using Template.MobileApp.Helpers;

public sealed partial class InputBehavior : BehaviorBase<VisualElement>, IInputHandler
{
    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(
        nameof(IsBusy),
        typeof(bool),
        typeof(InputBehavior),
        false);

    private bool closing;

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    bool IInputHandler.IsBusy => IsBusy || closing;

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);

        if (bindable is Page page)
        {
            page.Appearing += BindableOnActivated;
            page.Disappearing += BindableOnDeactivated;
        }
        else if (bindable is Popup popup)
        {
            popup.Opened += PopupOnOpened;
            popup.Closed += BindableOnDeactivated;
        }
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        if (bindable is Page page)
        {
            page.Appearing -= BindableOnActivated;
            page.Disappearing -= BindableOnDeactivated;
        }
        else if (bindable is Popup popup)
        {
            popup.Opened -= PopupOnOpened;
            popup.Closed -= BindableOnDeactivated;
        }

        InputManager.Default.PopHandler(this);

        base.OnDetachingFrom(bindable);
    }

    private void BindableOnActivated(object? sender, EventArgs e)
    {
        InputManager.Default.PushHandler(this);
    }

    private void BindableOnDeactivated(object? sender, EventArgs e)
    {
        InputManager.Default.PopHandler(this);
    }

    private void PopupOnOpened(object? sender, EventArgs e)
    {
        InputManager.Default.PushHandler(this);
        AttachPlatformKeyListener((Popup)sender!);
    }

    private static partial void AttachPlatformKeyListener(Popup popup);

    //--------------------------------------------------------------------------------
    // Input
    //--------------------------------------------------------------------------------

    public bool CanHandle(ShortcutKey key)
    {
        if (IsBusy || closing)
        {
            return true;
        }

        if ((AssociatedObject is null) || !AssociatedObject.IsEnabled)
        {
            return false;
        }

        var focused = FindFocused();
        if ((focused is not null) && AnyShortcutBehavior(focused, key, static (x, s) => x.CanHandle(s)))
        {
            return true;
        }

        return (key is ShortcutKey.Up or ShortcutKey.Down or ShortcutKey.Left or ShortcutKey.Right) ||
               (FindShortcut(AssociatedObject, key) is not null) ||
               ((key == ShortcutKey.Escape) && (AssociatedObject is Page or Popup));
    }

    public bool Handle(ShortcutKey key)
    {
        // Busy
        if (IsBusy || closing)
        {
            return true;
        }

        if ((AssociatedObject is null) || !AssociatedObject.IsEnabled)
        {
            return false;
        }

        var focused = FindFocused();
        if ((focused is not null) && AnyShortcutBehavior(focused, key, static (x, s) => x.Handle(s)))
        {
            return true;
        }

        // Move focus
        if (key is ShortcutKey.Up or ShortcutKey.Down or ShortcutKey.Left or ShortcutKey.Right)
        {
            ElementHelper.MoveFocus(ElementHelper.ResolveScope(AssociatedObject), key is ShortcutKey.Down or ShortcutKey.Right);
            return true;
        }

        var element = FindShortcut(AssociatedObject, key);
        if (element is not null)
        {
            InvokeShortcut(element);
            return true;
        }

        // Back
        if ((key == ShortcutKey.Escape) && (AssociatedObject is Page page))
        {
            page.SendBackButtonPressed();
            return true;
        }

        // Close popup
        if ((key == ShortcutKey.Escape) && (AssociatedObject is Popup popup))
        {
            closing = true;
            _ = popup.CloseAsync();
            return true;
        }

        return false;
    }

    public VisualElement? FindFocused() =>
        AssociatedObject is not null ? ElementHelper.FindFocused(AssociatedObject) : null;

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static bool AnyShortcutBehavior(VisualElement element, ShortcutKey key, Func<IShortcutBehavior, ShortcutKey, bool> predicate)
    {
        var behaviors = element.Behaviors;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < behaviors.Count; i++)
        {
            if ((behaviors[i] is IShortcutBehavior behavior) && predicate(behavior, key))
            {
                return true;
            }
        }

        return false;
    }

    private static VisualElement? FindShortcut(VisualElement parent, ShortcutKey key) =>
        ElementHelper.FindActive(parent, key, static (x, s) => Shortcut.GetKey(x) == s);

    private static void InvokeShortcut(VisualElement element)
    {
        switch (element)
        {
            case IButtonController button:
                button.SendClicked();
                break;
            case CheckBox checkBox:
                checkBox.IsChecked = !checkBox.IsChecked;
                break;
            case Switch toggle:
                toggle.IsToggled = !toggle.IsToggled;
                break;
            case RadioButton radioButton:
                radioButton.IsChecked = true;
                break;
            default:
                ElementHelper.RequestFocus(element);
                break;
        }
    }
}
