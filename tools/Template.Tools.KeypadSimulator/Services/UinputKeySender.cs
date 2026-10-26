namespace Template.Tools.KeypadSimulator.Services;

// ReSharper disable StringLiteralTypo
public sealed class UinputKeySender : IKeySender
{
    private const int KeysDeviceId = 1;
    private const int CenterDeviceId = 2;
    private const string KeysDeviceName = "Keypad Simulator Keys";
    private const string CenterDeviceName = "Keypad Simulator Center";
    private const int VendorId = 0x1209;
    private const int KeysProductId = 0x0001;
    private const int CenterProductId = 0x0002;

    private static readonly TimeSpan RegisterTimeout = TimeSpan.FromSeconds(5);

    private static readonly string[] AlphabeticKeys =
    [
        "KEY_Q", "KEY_W", "KEY_E", "KEY_R", "KEY_T", "KEY_Y", "KEY_U", "KEY_I", "KEY_O", "KEY_P",
        "KEY_A", "KEY_S", "KEY_D", "KEY_F", "KEY_G", "KEY_H", "KEY_J", "KEY_K", "KEY_L",
        "KEY_Z", "KEY_X", "KEY_C", "KEY_V", "KEY_B", "KEY_N", "KEY_M",
        "KEY_SPACE", "KEY_TAB", "KEY_LEFTSHIFT"
    ];

    private readonly AdbService adbService;

    private readonly KeypadKeyMap keyMap;

    private readonly KeyboardType keyboardType;

    private readonly Dictionary<KeypadKey, (int Device, string Code)> pressedKeys = [];

    private AdbSession? session;

    public event EventHandler? Disconnected;

    public string Description => keyboardType == KeyboardType.Alphabetic ? "Hardware (alphabetic)" : "Hardware (numeric)";

    public UinputKeySender(AdbService adbService, KeypadKeyMap keyMap, KeyboardType keyboardType)
    {
        this.adbService = adbService;
        this.keyMap = keyMap;
        this.keyboardType = keyboardType;
    }

    public void Dispose()
    {
        if (session is not null)
        {
            ReleaseAll();
            session.Exited -= OnExited;
            session.Dispose();
            session = null;
        }
    }

    public async ValueTask<bool> ConnectAsync(string serial, CancellationToken cancellationToken)
    {
        session = adbService.StartShell(serial, "uinput", "-");
        session.Exited += OnExited;

        var keys = keyMap.All
            .Where(static x => !x.IsCenter)
            .SelectMany(static x => x.NumpadLinuxKey is null ? [x.LinuxKey] : (IEnumerable<string>)[x.LinuxKey, x.NumpadLinuxKey]);
        if (keyboardType == KeyboardType.Alphabetic)
        {
            keys = keys.Concat(AlphabeticKeys);
        }

        if (!session.WriteLine(MakeRegister(KeysDeviceId, KeysDeviceName, KeysProductId, keys.Distinct())) ||
            !session.WriteLine(MakeRegister(CenterDeviceId, CenterDeviceName, CenterProductId, [KeypadKey.CenterLinuxKey])))
        {
            return false;
        }

        return await adbService.WaitForInputDevicesAsync(serial, [KeysDeviceName, CenterDeviceName], RegisterTimeout, cancellationToken);
    }

    public string? Press(KeypadKey key, bool numpad)
    {
        if (pressedKeys.ContainsKey(key))
        {
            return null;
        }

        var device = key.IsCenter ? CenterDeviceId : KeysDeviceId;
        var code = numpad ? key.NumpadLinuxKey ?? key.LinuxKey : key.LinuxKey;
        if (!Inject(device, code, 1))
        {
            return null;
        }

        pressedKeys[key] = (device, code);
        return code;
    }

    public void Release(KeypadKey key)
    {
        if (pressedKeys.Remove(key, out var pressed))
        {
            Inject(pressed.Device, pressed.Code, 0);
        }
    }

    public void ReleaseAll()
    {
        foreach (var (device, code) in pressedKeys.Values)
        {
            Inject(device, code, 0);
        }

        pressedKeys.Clear();
    }

    private bool Inject(int device, string code, int value) =>
        session?.WriteLine(String.Create(CultureInfo.InvariantCulture, $"{{\"id\":{device},\"command\":\"inject\",\"events\":[\"EV_KEY\",\"{code}\",{value},\"EV_SYN\",\"SYN_REPORT\",0]}}")) ?? false;

    private static string MakeRegister(int id, string name, int productId, IEnumerable<string> keys) =>
        String.Create(CultureInfo.InvariantCulture, $"{{\"id\":{id},\"command\":\"register\",\"name\":\"{name}\",\"vid\":{VendorId},\"pid\":{productId},\"bus\":\"usb\",\"configuration\":[{{\"type\":\"UI_SET_EVBIT\",\"data\":[\"EV_KEY\"]}},{{\"type\":\"UI_SET_KEYBIT\",\"data\":[{String.Join(',', keys.Select(static x => $"\"{x}\""))}]}}]}}");

    private void OnExited(object? sender, EventArgs e) => Disconnected?.Invoke(this, EventArgs.Empty);
}
// ReSharper restore StringLiteralTypo
