namespace Template.Tools.KeypadSimulator.Models;

// ReSharper disable StringLiteralTypo
public static class KeypadKeys
{
    public static IReadOnlyList<KeypadKey> Defaults { get; } =
    [
        // Function
        new(KeypadKeyId.F1, "F1", KeyKind.Function1, "KEYCODE_F1", "KEY_F1"),
        new(KeypadKeyId.F2, "F2", KeyKind.Function2, "KEYCODE_F2", "KEY_F2"),
        new(KeypadKeyId.F3, "F3", KeyKind.Function3, "KEYCODE_F3", "KEY_F3"),
        new(KeypadKeyId.F4, "F4", KeyKind.Function4, "KEYCODE_F4", "KEY_F4"),

        // Control
        new(KeypadKeyId.Escape, "ESC", KeyKind.Control, "KEYCODE_ESCAPE", "KEY_ESC"),
        new(KeypadKeyId.Up, "▲", KeyKind.Control, "KEYCODE_DPAD_UP", "KEY_UP"),
        new(KeypadKeyId.Space, "SPC", KeyKind.Control, "KEYCODE_SPACE", "KEY_SPACE"),
        new(KeypadKeyId.Left, "◀", KeyKind.Control, "KEYCODE_DPAD_LEFT", "KEY_LEFT"),
        new(KeypadKeyId.Center, "●", KeyKind.Control, "KEYCODE_DPAD_CENTER", KeypadKey.CenterLinuxKey),
        new(KeypadKeyId.Right, "▶", KeyKind.Control, "KEYCODE_DPAD_RIGHT", "KEY_RIGHT"),
        new(KeypadKeyId.Del, "DEL", KeyKind.Control, "KEYCODE_DEL", "KEY_BACKSPACE"),
        new(KeypadKeyId.Down, "▼", KeyKind.Control, "KEYCODE_DPAD_DOWN", "KEY_DOWN"),
        new(KeypadKeyId.Enter, "ENT", KeyKind.Control, "KEYCODE_ENTER", "KEY_ENTER", "KEYCODE_NUMPAD_ENTER", "KEY_KPENTER"),

        // Number
        MakeNumber(KeypadKeyId.Num1, 1),
        MakeNumber(KeypadKeyId.Num2, 2),
        MakeNumber(KeypadKeyId.Num3, 3),
        MakeNumber(KeypadKeyId.Num4, 4),
        MakeNumber(KeypadKeyId.Num5, 5),
        MakeNumber(KeypadKeyId.Num6, 6),
        MakeNumber(KeypadKeyId.Num7, 7),
        MakeNumber(KeypadKeyId.Num8, 8),
        MakeNumber(KeypadKeyId.Num9, 9),
        new(KeypadKeyId.Minus, "-", KeyKind.Number, "KEYCODE_MINUS", "KEY_MINUS", "KEYCODE_NUMPAD_SUBTRACT", "KEY_KPMINUS"),
        MakeNumber(KeypadKeyId.Num0, 0),
        new(KeypadKeyId.Period, ".", KeyKind.Number, "KEYCODE_PERIOD", "KEY_DOT", "KEYCODE_NUMPAD_DOT", "KEY_KPDOT")
    ];

    private static KeypadKey MakeNumber(KeypadKeyId id, int number)
    {
        var text = number.ToString(CultureInfo.InvariantCulture);
        return new(id, text, KeyKind.Number, $"KEYCODE_{text}", $"KEY_{text}", $"KEYCODE_NUMPAD_{text}", $"KEY_KP{text}");
    }
}
// ReSharper restore StringLiteralTypo
