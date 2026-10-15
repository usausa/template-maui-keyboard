namespace Template.MobileApp.Helpers;

using CommunityToolkit.Maui.Views;

using Microsoft.Maui;

using Template.MobileApp.Behaviors;

public static partial class ElementHelper
{
    public static VisualElement? FindFocused(IVisualTreeElement parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
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

    public static IEnumerable<VisualElement> EnumerateFocusable(IVisualTreeElement parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is not VisualElement { IsEnabled: true, IsVisible: true } visual)
            {
                continue;
            }

            // ScrollView はキーのスクロールのためにフォーカスを持てるが、移る先は中の要素
            if ((visual is not ScrollView) && IsPlatformFocusable(visual))
            {
                yield return visual;
            }

            foreach (var descendant in EnumerateFocusable(child))
            {
                yield return descendant;
            }
        }
    }

    private static partial bool IsPlatformFocusable(VisualElement visual);

    public static IEnumerable<T> EnumerateActive<T>(IVisualTreeElement parent)
    {
        foreach (var child in parent.GetVisualChildren())
        {
            if (child is not VisualElement { IsEnabled: true, IsVisible: true })
            {
                continue;
            }

            if (child is T element)
            {
                yield return element;
            }

            foreach (var descendant in EnumerateActive<T>(child))
            {
                yield return descendant;
            }
        }
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
        var elements = EnumerateFocusable(root).ToList();
        var index = current is not null ? elements.IndexOf(current) : -1;
        var next = FindNext(elements, index, forward);
        if ((next < 0) && (current is not null) && IsWrap(current, root))
        {
            next = FindNext(elements, -1, forward);
        }

        if ((next < 0) || (next == index))
        {
            return false;
        }

        RequestFocus(elements[next]);
        return true;
    }

    private static int FindNext(List<VisualElement> elements, int index, bool forward)
    {
        var step = forward ? 1 : -1;
        var start = index >= 0 ? index + step : forward ? 0 : elements.Count - 1;
        for (var i = start; (i >= 0) && (i < elements.Count); i += step)
        {
            if (!Focus.GetSkip(elements[i]))
            {
                return i;
            }
        }

        return -1;
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
        foreach (var child in parent.GetVisualChildren())
        {
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
