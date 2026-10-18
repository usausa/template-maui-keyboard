namespace Template.Tools.KeypadSimulator.Services;

public interface IKeySender : IDisposable
{
    event EventHandler? Disconnected;

    string Description { get; }

    ValueTask<bool> ConnectAsync(string serial, CancellationToken cancellationToken);

    string? Press(KeypadKey key, bool numpad);

    void Release(KeypadKey key);

    void ReleaseAll();
}
