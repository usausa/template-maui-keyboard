namespace Template.MobileApp.Helpers;

using CommunityToolkit.Maui.Views;

using Microsoft.Maui;

using Template.MobileApp.Behaviors;

public static partial class ElementHelper
{
    public static VisualElement? FindFocused(IVisualTreeElement parent)
    {
        var children = parent.GetVisualChildren();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if ((child is VisualElement visual) && visual.IsFocused)
            {
                return visual;
            }

            var focused = FindFocused(child);
            if (focused is not null)
            {
                return focused;
            }

            // Android の側の子がフォーカスを持つコントロール(Stepper など)
            if ((child is VisualElement container) && IsPlatformFocusWithin(container))
            {
                return container;
            }
        }

        return null;
    }

    private static partial bool IsPlatformFocusWithin(VisualElement visual);

    // フォーカスできる要素を XAML の順にたどり、条件に合う最初の要素を返す
    public static VisualElement? FindFocusable(IVisualTreeElement parent, Func<VisualElement, bool> predicate) =>
        FindActive(parent, predicate, static (x, p) => IsFocusTarget(x) && p(x));

    // ScrollView はキーのスクロールのためにフォーカスを持てるが、移る先は中の要素
    private static bool IsFocusTarget(VisualElement visual) =>
        (visual is not ScrollView) && IsPlatformFocusable(visual);

    private static partial bool IsPlatformFocusable(VisualElement visual);

    // 有効で見えている要素を XAML の順にたどり、条件に合う最初の要素を返す(無効・非表示の要素の中は見ない)
    public static VisualElement? FindActive<TState>(IVisualTreeElement parent, TState state, Func<VisualElement, TState, bool> predicate)
    {
        var children = parent.GetVisualChildren();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < children.Count; i++)
        {
            if (children[i] is not VisualElement { IsEnabled: true, IsVisible: true } visual)
            {
                continue;
            }

            if (predicate(visual, state))
            {
                return visual;
            }

            var found = FindActive(visual, state, predicate);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    // タッチの後でもボタンにフォーカスを置き、キーをアプリで受け取る(入力欄は MAUI の Focus でソフトキーボードも扱う)
    public static void RequestFocus(VisualElement element)
    {
        if (element is InputView)
        {
            element.Focus();
        }
        else
        {
            PlatformRequestFocus(element);
        }
    }

    private static partial void PlatformRequestFocus(VisualElement element);

    // タッチで操作している間か(Android のタッチモード)
    public static partial bool IsInTouchMode(VisualElement element);

    // 上下左右のフォーカスの移動: XAML に書いた順で、Focus.Skip の要素は飛ばし、端では Focus.Wrap があれば折り返す(無ければ止める)
    public static bool MoveFocus(VisualElement root, bool forward) =>
        MoveFocus(root, FindFocused(root), forward);

    public static bool MoveFocusInRoot(VisualElement current, bool forward)
    {
        var root = current.FindRoot();
        return (root is not null) && MoveFocus(root, current, forward);
    }

    private static bool MoveFocus(VisualElement root, VisualElement? current, bool forward)
    {
        var search = new FocusSearch(current, forward);
        search.Visit(root);
        var next = search.Next;
        if ((next is null) && (current is not null) && IsWrap(current, root))
        {
            next = search.Wrap;
        }

        if (next is null)
        {
            return false;
        }

        RequestFocus(next);
        return true;
    }

    // フォーカスできる要素を XAML の順にたどり、今の要素の前後で Focus.Skip でない要素を探す(一覧は作らない)
    private struct FocusSearch
    {
        private readonly VisualElement? current;

        private readonly bool forward;

        private bool passed;

        // 今の要素より前(前へは直前、後ろへは先頭)
        private VisualElement? before;

        // 今の要素より後(後ろへは直後、前へは末尾)
        private VisualElement? after;

        public FocusSearch(VisualElement? current, bool forward)
        {
            this.current = current;
            this.forward = forward;
        }

        // 移る先(今の要素が無い時は、後ろへは先頭、前へは末尾)
        public readonly VisualElement? Next => (passed && forward) ? after : before;

        // 端で折り返す先
        public readonly VisualElement? Wrap => forward ? before : after;

        public bool Visit(IVisualTreeElement parent)
        {
            var children = parent.GetVisualChildren();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] is not VisualElement { IsEnabled: true, IsVisible: true } visual)
                {
                    continue;
                }

                if (IsFocusTarget(visual) && Collect(visual))
                {
                    return true;
                }

                if (Visit(visual))
                {
                    return true;
                }
            }

            return false;
        }

        // 探し終えたら true
        private bool Collect(VisualElement element)
        {
            if (ReferenceEquals(element, current))
            {
                passed = true;
                return !forward && (before is not null);
            }

            if (Focus.GetSkip(element))
            {
                return false;
            }

            if (passed)
            {
                after = element;
                return forward;
            }

            if (!forward || (before is null))
            {
                before = element;
            }

            return false;
        }
    }

    private static bool IsWrap(Element element, Element root)
    {
        for (var current = element; current is not null; current = current.Parent)
        {
            if (Focus.GetWrap(current))
            {
                return true;
            }

            if (ReferenceEquals(current, root))
            {
                break;
            }
        }

        return false;
    }

    // 上下左右で移る範囲(Focus.Scope を付けた要素・ページ・ポップアップ)
    public static VisualElement? FindRoot(this Element element)
    {
        for (var parent = element.Parent; parent is not null; parent = parent.Parent)
        {
            if ((parent is VisualElement visual) && (Focus.GetScope(visual) || (parent is Page) || (parent is Popup)))
            {
                return visual;
            }
        }

        return null;
    }

    // Focus.Scope を付けた子孫(無ければ自分)
    public static VisualElement ResolveScope(VisualElement element) =>
        FindScope(element) ?? element;

    private static VisualElement? FindScope(IVisualTreeElement parent)
    {
        var children = parent.GetVisualChildren();
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if ((child is VisualElement visual) && Focus.GetScope(visual))
            {
                return visual;
            }

            var scope = FindScope(child);
            if (scope is not null)
            {
                return scope;
            }
        }

        return null;
    }
}
