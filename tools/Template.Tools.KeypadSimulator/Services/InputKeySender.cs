namespace Template.Tools.KeypadSimulator.Services;

public sealed class InputKeySender : IKeySender
{
    private readonly AdbService adbService;

    private AdbSession? session;

    public event EventHandler? Disconnected;

    public string Description => "Input";

    public InputKeySender(AdbService adbService)
    {
        this.adbService = adbService;
    }

    public void Dispose()
    {
        if (session is not null)
        {
            session.Exited -= OnExited;
            session.Dispose();
            session = null;
        }
    }

    public ValueTask<bool> ConnectAsync(string serial, CancellationToken cancellationToken)
    {
        session = adbService.StartShell(serial);
        session.Exited += OnExited;
        return ValueTask.FromResult(!session.HasExited);
    }

    public string? Press(KeypadKey key, bool numpad)
    {
        var code = numpad ? key.NumpadAndroidKey ?? key.AndroidKey : key.AndroidKey;
        return (session?.WriteLine($"input keyevent {code}") ?? false) ? code : null;
    }

    public void Release(KeypadKey key)
    {
    }

    public void ReleaseAll()
    {
    }

    private void OnExited(object? sender, EventArgs e) => Disconnected?.Invoke(this, EventArgs.Empty);
}
