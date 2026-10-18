namespace Template.Tools.KeypadSimulator.Models;

// ReSharper disable StringLiteralTypo
public static class KeypadKeys
{
    //--------------------------------------------------------------------------------
    // Function
    //--------------------------------------------------------------------------------

    public static KeypadKey F1 { get; } = new("F1", KeyKind.Function1, "KEYCODE_F1", "KEY_F1");

    public static KeypadKey F2 { get; } = new("F2", KeyKind.Function2, "KEYCODE_F2", "KEY_F2");

    public static KeypadKey F3 { get; } = new("F3", KeyKind.Function3, "KEYCODE_F3", "KEY_F3");

    public static KeypadKey F4 { get; } = new("F4", KeyKind.Function4, "KEYCODE_F4", "KEY_F4");

    //--------------------------------------------------------------------------------
    // Control
    //--------------------------------------------------------------------------------

    public static KeypadKey Escape { get; } = new("ESC", KeyKind.Control, "KEYCODE_ESCAPE", "KEY_ESC");

    public static KeypadKey Up { get; } = new("▲", KeyKind.Control, "KEYCODE_DPAD_UP", "KEY_UP");

    public static KeypadKey Left { get; } = new("◀", KeyKind.Control, "KEYCODE_DPAD_LEFT", "KEY_LEFT");

    public static KeypadKey Center { get; } = new("●", KeyKind.Control, "KEYCODE_DPAD_CENTER", KeypadKey.CenterLinuxKey);

    public static KeypadKey Right { get; } = new("▶", KeyKind.Control, "KEYCODE_DPAD_RIGHT", "KEY_RIGHT");

    public static KeypadKey Del { get; } = new("DEL", KeyKind.Control, "KEYCODE_DEL", "KEY_BACKSPACE");

    public static KeypadKey Down { get; } = new("▼", KeyKind.Control, "KEYCODE_DPAD_DOWN", "KEY_DOWN");

    public static KeypadKey Enter { get; } = new("ENT", KeyKind.Control, "KEYCODE_ENTER", "KEY_ENTER", "KEYCODE_NUMPAD_ENTER", "KEY_KPENTER");

    //--------------------------------------------------------------------------------
    // Number
    //--------------------------------------------------------------------------------

    public static KeypadKey Num1 { get; } = MakeNumber(1);

    public static KeypadKey Num2 { get; } = MakeNumber(2);

    public static KeypadKey Num3 { get; } = MakeNumber(3);

    public static KeypadKey Num4 { get; } = MakeNumber(4);

    public static KeypadKey Num5 { get; } = MakeNumber(5);

    public static KeypadKey Num6 { get; } = MakeNumber(6);

    public static KeypadKey Num7 { get; } = MakeNumber(7);

    public static KeypadKey Num8 { get; } = MakeNumber(8);

    public static KeypadKey Num9 { get; } = MakeNumber(9);

    public static KeypadKey Num0 { get; } = MakeNumber(0);

    public static KeypadKey Minus { get; } = new("-", KeyKind.Number, "KEYCODE_MINUS", "KEY_MINUS", "KEYCODE_NUMPAD_SUBTRACT", "KEY_KPMINUS");

    public static KeypadKey Period { get; } = new(".", KeyKind.Number, "KEYCODE_PERIOD", "KEY_DOT", "KEYCODE_NUMPAD_DOT", "KEY_KPDOT");

    //--------------------------------------------------------------------------------
    // All
    //--------------------------------------------------------------------------------

    public static IReadOnlyList<KeypadKey> All { get; } =
    [
        F1, F2, F3, F4,
        Escape, Up, Left, Center, Right, Del, Down, Enter,
        Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9, Num0, Minus, Period
    ];

    private static KeypadKey MakeNumber(int number)
    {
        var text = number.ToString(CultureInfo.InvariantCulture);
        return new(text, KeyKind.Number, $"KEYCODE_{text}", $"KEY_{text}", $"KEYCODE_NUMPAD_{text}", $"KEY_KP{text}");
    }
}
// ReSharper restore StringLiteralTypo
