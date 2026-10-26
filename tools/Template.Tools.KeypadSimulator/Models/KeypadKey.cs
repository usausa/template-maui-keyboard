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

public enum KeypadKeyId
{
    F1,
    F2,
    F3,
    F4,
    Escape,
    Up,
    Space,
    Left,
    Center,
    Right,
    Del,
    Down,
    Enter,
    Num1,
    Num2,
    Num3,
    Num4,
    Num5,
    Num6,
    Num7,
    Num8,
    Num9,
    Minus,
    Num0,
    Period
}

public sealed record KeypadKey(
    KeypadKeyId Id,
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

public sealed record PcKeyInput(KeypadKeyId Id, bool Numpad, bool IsRepeat);
