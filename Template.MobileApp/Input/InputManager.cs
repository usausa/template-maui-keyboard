namespace Template.MobileApp.Input;

public sealed class InputManager
{
    public static InputManager Default { get; } = new();

    private readonly List<IInputHandler> handlers = [];

    public void PushHandler(IInputHandler handler)
    {
        handlers.Remove(handler);
        handlers.Add(handler);
    }

    public void PopHandler(IInputHandler handler) => handlers.Remove(handler);

    public bool IsBusy => (handlers.Count > 0) && handlers[^1].IsBusy;

    public bool CanProcess(ShortcutKey key) => (handlers.Count > 0) && handlers[^1].CanHandle(key);

    public bool Process(ShortcutKey key) => (handlers.Count > 0) && handlers[^1].Handle(key);

    public VisualElement? FindFocused() => handlers.Count > 0 ? handlers[^1].FindFocused() : null;
}
