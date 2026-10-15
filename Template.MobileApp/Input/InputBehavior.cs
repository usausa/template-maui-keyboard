namespace Template.MobileApp.Input;

using CommunityToolkit.Maui.Views;

using Smart.Maui.Interactivity;

using Template.MobileApp.Helpers;

public sealed partial class InputBehavior : BehaviorBase<VisualElement>, IInputHandler
{
    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(
        nameof(IsBusy),
        typeof(bool),
        typeof(InputBehavior),
        false);

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

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

    // ポップアップは別のウィンドウなので、そのウィンドウのキーを受ける
    private static partial void AttachPlatformKeyListener(Popup popup);

    public bool CanHandle(ShortcutKey key)
    {
        if (IsBusy)
        {
            return true;
        }

        if ((AssociatedObject is null) || !AssociatedObject.IsEnabled)
        {
            return false;
        }

        var focused = FindFocused();
        if ((focused is not null) && focused.Behaviors.OfType<IShortcutBehavior>().Any(x => x.CanHandle(key)))
        {
            return true;
        }

        return (key is ShortcutKey.Up or ShortcutKey.Down) || (FindShortcutButton(AssociatedObject, key) is not null);
    }

    public bool Handle(ShortcutKey key)
    {
        // 処理中のキーは捨てる
        if (IsBusy)
        {
            return true;
        }

        if ((AssociatedObject is null) || !AssociatedObject.IsEnabled)
        {
            return false;
        }

        var focused = FindFocused();
        if ((focused is not null) && focused.Behaviors.OfType<IShortcutBehavior>().Any(x => x.Handle(key)))
        {
            return true;
        }

        if (key is ShortcutKey.Up or ShortcutKey.Down)
        {
            return ElementHelper.MoveFocus(AssociatedObject, key == ShortcutKey.Down);
        }

        var button = FindShortcutButton(AssociatedObject, key);
        if (button is not null)
        {
            button.SendClicked();
            return true;
        }

        return false;
    }

    public VisualElement? FindFocused() =>
        AssociatedObject is not null ? ElementHelper.FindFocused(AssociatedObject) : null;

    private static Button? FindShortcutButton(VisualElement parent, ShortcutKey key) =>
        ElementHelper.EnumerateActive<Button>(parent).FirstOrDefault(key, static (x, s) => Shortcut.GetKey(x) == s);
}
