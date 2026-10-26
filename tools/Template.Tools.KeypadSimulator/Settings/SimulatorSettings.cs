namespace Template.Tools.KeypadSimulator.Settings;

public sealed class KeySetting
{
    public string? Label { get; set; }

    public string? AndroidKey { get; set; }

    public string? LinuxKey { get; set; }

    public string? NumpadAndroidKey { get; set; }

    public string? NumpadLinuxKey { get; set; }
}

public sealed class SimulatorSettings
{
    public string? AdbPath { get; set; }

    public Dictionary<string, KeySetting> Keys { get; } = [];
}
