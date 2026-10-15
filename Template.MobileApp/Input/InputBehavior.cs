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

    private bool closing;

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
        if (IsBusy || closing)
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

        return (key is ShortcutKey.Up or ShortcutKey.Down or ShortcutKey.Left or ShortcutKey.Right) ||
               (FindShortcut(AssociatedObject, key) is not null) ||
               ((key == ShortcutKey.Escape) && (AssociatedObject is Page or Popup));
    }

    public bool Handle(ShortcutKey key)
    {
        // 処理中と閉じている途中のキーは捨てる
        if (IsBusy || closing)
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

        // 端で止める時もキーは使ったものとし、範囲の外へフォーカスを出さない
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

        // 画面の Esc は Android の戻るキーと同じ
        if ((key == ShortcutKey.Escape) && (AssociatedObject is Page page))
        {
            page.SendBackButtonPressed();
            return true;
        }

        // ポップアップの Esc はキャンセルで閉じる(結果は無し)
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

    private static VisualElement? FindShortcut(VisualElement parent, ShortcutKey key) =>
        ElementHelper.EnumerateActive<VisualElement>(parent).FirstOrDefault(key, static (x, s) => Shortcut.GetKey(x) == s);

    // タッチと同じ操作をする(フォーカスは動かさない)
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
