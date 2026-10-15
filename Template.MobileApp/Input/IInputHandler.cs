namespace Template.MobileApp.Input;

public interface IInputHandler
{
    bool CanHandle(ShortcutKey key);

    bool Handle(ShortcutKey key);

    VisualElement? FindFocused();
}
