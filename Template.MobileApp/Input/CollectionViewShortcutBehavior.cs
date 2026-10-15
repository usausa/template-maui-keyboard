namespace Template.MobileApp.Input;

using Smart.Maui.Interactivity;

public sealed partial class CollectionViewShortcutBehavior : BehaviorBase<CollectionView>, IShortcutBehavior
{
    public static readonly BindableProperty KeyCodeProperty = BindableProperty.Create(
        nameof(KeyCode),
        typeof(ShortcutKey),
        typeof(CollectionViewShortcutBehavior));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(CollectionViewShortcutBehavior));

    public static readonly BindableProperty SelectByNumberKeyProperty = BindableProperty.Create(
        nameof(SelectByNumberKey),
        typeof(bool),
        typeof(CollectionViewShortcutBehavior),
        false);

    public ShortcutKey KeyCode
    {
        get => (ShortcutKey)GetValue(KeyCodeProperty);
        set => SetValue(KeyCodeProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    // 数字キーの 1〜9 で 1〜9 行目を選ぶ
    public bool SelectByNumberKey
    {
        get => (bool)GetValue(SelectByNumberKeyProperty);
        set => SetValue(SelectByNumberKeyProperty, value);
    }

    protected override void OnAttachedTo(CollectionView bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.HandlerChanged += BindableOnHandlerChanged;
        bindable.Focused += BindableOnFocused;
        UpdatePlatformFocus(bindable);
    }

    protected override void OnDetachingFrom(CollectionView bindable)
    {
        bindable.HandlerChanged -= BindableOnHandlerChanged;
        bindable.Focused -= BindableOnFocused;
        base.OnDetachingFrom(bindable);
    }

    private static void BindableOnHandlerChanged(object? sender, EventArgs e)
    {
        UpdatePlatformFocus((CollectionView)sender!);
    }

    // 行ではなく一覧にフォーカスを置き、行はこの Behavior で選ぶ
    private static partial void UpdatePlatformFocus(CollectionView view);

    // フォーカスが来た時にどの行も選ばれていなければ、先頭の行を選ぶ
    private void BindableOnFocused(object? sender, FocusEventArgs e)
    {
        if ((AssociatedObject is { SelectedItem: null } view) && (view.ItemsSource is IList { Count: > 0 } items))
        {
            Select(view, items, 0);
        }
    }

    public bool CanHandle(ShortcutKey key)
    {
        if ((AssociatedObject is not { } view) || (view.ItemsSource is not IList items))
        {
            return false;
        }

        if (key is ShortcutKey.Up or ShortcutKey.Down)
        {
            return ResolveNextIndex(view, items, key) >= 0;
        }

        return IsNumberKey(key) || ((key == KeyCode) && (Command is not null));
    }

    public bool Handle(ShortcutKey key)
    {
        if ((AssociatedObject is not { } view) || (view.ItemsSource is not IList items))
        {
            return false;
        }

        // 端の行では処理せず、一覧の外へフォーカスを移す
        if (key is ShortcutKey.Up or ShortcutKey.Down)
        {
            var index = ResolveNextIndex(view, items, key);
            if (index < 0)
            {
                return false;
            }

            Select(view, items, index);
            return true;
        }

        // 行の無い番号では何もしない(画面のショートカットには回さない)
        if (IsNumberKey(key))
        {
            var number = key - ShortcutKey.Num1;
            if (number < items.Count)
            {
                Select(view, items, number);
            }

            return true;
        }

        if ((key != KeyCode) || (Command is not { } command))
        {
            return false;
        }

        var item = view.SelectedItem;
        if ((item is not null) && command.CanExecute(item))
        {
            command.Execute(item);
        }

        return true;
    }

    private bool IsNumberKey(ShortcutKey key) =>
        SelectByNumberKey && (key >= ShortcutKey.Num1) && (key <= ShortcutKey.Num9);

    private static int ResolveNextIndex(CollectionView view, IList items, ShortcutKey key)
    {
        var index = view.SelectedItem is not null ? items.IndexOf(view.SelectedItem) : -1;
        var next = key == ShortcutKey.Up ? index - 1 : index + 1;
        return (next >= 0) && (next < items.Count) ? next : -1;
    }

    private static void Select(CollectionView view, IList items, int index)
    {
        var item = items[index];
        view.SelectedItem = item;
        view.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false);
    }
}
