namespace Template.Tools.KeypadSimulator.Services;

using Template.Tools.KeypadSimulator.Settings;

public sealed class KeypadKeyMap
{
    private readonly Dictionary<KeypadKeyId, KeypadKey> keys;

    public IReadOnlyList<KeypadKey> All { get; }

    public KeypadKey this[KeypadKeyId id] => keys[id];

    public KeypadKeyMap(
        ILogger<KeypadKeyMap> log,
        SimulatorSettings settings)
    {
        var configured = new Dictionary<string, KeySetting>(settings.Keys, StringComparer.OrdinalIgnoreCase);
        foreach (var name in configured.Keys.Where(static x => !Enum.TryParse<KeypadKeyId>(x, true, out _)))
        {
            log.WarnUnknownKeyName(name);
        }

        All = [.. KeypadKeys.Defaults.Select(x => configured.TryGetValue(x.Id.ToString(), out var setting) ? Apply(x, setting) : x)];
        keys = All.ToDictionary(static x => x.Id);
    }

    private static KeypadKey Apply(KeypadKey key, KeySetting setting) =>
        key with
        {
            Label = setting.Label ?? key.Label,
            AndroidKey = setting.AndroidKey ?? key.AndroidKey,
            LinuxKey = setting.LinuxKey ?? key.LinuxKey,
            NumpadAndroidKey = setting.NumpadAndroidKey ?? key.NumpadAndroidKey,
            NumpadLinuxKey = setting.NumpadLinuxKey ?? key.NumpadLinuxKey
        };
}
