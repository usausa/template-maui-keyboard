namespace Template.Tools.KeypadSimulator.Models;

public enum KeyKind
{
    Number,
    Control,
    Function1,
    Function2,
    Function3,
    Function4
}

public enum SendMode
{
    Hardware,
    Input
}

public enum KeyboardType
{
    Alphabetic,
    Numeric
}

public sealed record KeypadKey(
    string Label,
    KeyKind Kind,
    string AndroidKey,
    string LinuxKey,
    string? NumpadAndroidKey = null,
    string? NumpadLinuxKey = null)
{
    public const string CenterLinuxKey = "KEY_SELECT";

    public bool IsCenter => LinuxKey == CenterLinuxKey;
}

public sealed record PcKeyInput(KeypadKey Key, bool Numpad, bool IsRepeat);
