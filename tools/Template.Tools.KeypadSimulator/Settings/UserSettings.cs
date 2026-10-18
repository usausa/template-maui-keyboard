namespace Template.Tools.KeypadSimulator.Settings;

using System.Configuration;

public sealed class MainWindowPlacement
{
    public int Left { get; set; }

    public int Top { get; set; }
}

public sealed class UserSettings : ApplicationSettingsBase
{
    [UserScopedSetting]
    public MainWindowPlacement? MainWindowPlacement
    {
        get => (MainWindowPlacement?)this[nameof(MainWindowPlacement)];
        set => this[nameof(MainWindowPlacement)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string Serial
    {
        get => (string)this[nameof(Serial)];
        set => this[nameof(Serial)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("Hardware")]
    public SendMode Mode
    {
        get => (SendMode)this[nameof(Mode)];
        set => this[nameof(Mode)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("Alphabetic")]
    public KeyboardType KeyboardType
    {
        get => (KeyboardType)this[nameof(KeyboardType)];
        set => this[nameof(KeyboardType)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool PcKeys
    {
        get => (bool)this[nameof(PcKeys)];
        set => this[nameof(PcKeys)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("False")]
    public bool Numpad
    {
        get => (bool)this[nameof(Numpad)];
        set => this[nameof(Numpad)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("False")]
    public bool Topmost
    {
        get => (bool)this[nameof(Topmost)];
        set => this[nameof(Topmost)] = value;
    }
}
