namespace Template.MobileApp.Input;

public interface IShortcutBehavior
{
    bool CanHandle(ShortcutKey key);

    bool Handle(ShortcutKey key);
}
