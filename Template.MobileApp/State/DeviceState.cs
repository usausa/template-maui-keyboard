namespace Template.MobileApp.State;

using Template.MobileApp.Components;

#pragma warning disable CA1008
[Flags]
public enum NetworkProfile
{
    Unknown = 0,
    Bluetooth = 0x01,
    Cellular = 0x02,
    Ethernet = 0x04,
    WiFi = 0x08
}
#pragma warning restore CA1008

public enum NetworkState
{
    Connected,
    ConnectedHighSpeed,
    Disconnected
}

public static class DeviceStateExtensions
{
    public static bool IsConnected(this NetworkAccess access) =>
        (access != NetworkAccess.None) && (access != NetworkAccess.Unknown);

    public static bool IsHighSpeed(this NetworkProfile profile) =>
        profile.HasFlag(NetworkProfile.Ethernet) || profile.HasFlag(NetworkProfile.WiFi);

    public static bool IsConnected(this NetworkState state) =>
        state is NetworkState.ConnectedHighSpeed or NetworkState.Connected;
}

public sealed partial class DeviceState : ObservableObject, IDisposable
{
    private readonly ILogger<DeviceState> log;

    private readonly DeviceInformation deviceInformation;

    // Battery

    [ObservableProperty]
    public partial double BatteryChargeLevel { get; private set; }

    [ObservableProperty]
    public partial BatteryState BatteryState { get; private set; }

    [ObservableProperty]
    public partial BatteryPowerSource BatteryPowerSource { get; private set; }

    // Connectivity

    [ObservableProperty]
    public partial NetworkProfile NetworkProfile { get; private set; }

    [ObservableProperty]
    public partial NetworkAccess NetworkAccess { get; private set; }

    [ObservableProperty]
    public partial NetworkState NetworkState { get; private set; }

    public int WiFiSignalStrength { get; private set; }

    public DeviceState(
        ILogger<DeviceState> log,
        DeviceInformation deviceInformation)
    {
        this.log = log;
        this.deviceInformation = deviceInformation;

        // Battery
        if (deviceInformation.Battery is { } battery)
        {
            UpdateBattery(battery);
        }

        deviceInformation.BatteryChanged += OnBatteryChanged;

        // Connectivity
        if (deviceInformation.Network is { } network)
        {
            UpdateConnectivity(network);
        }

        deviceInformation.NetworkChanged += OnNetworkChanged;

        UpdateWiFi(deviceInformation.WiFi);

        deviceInformation.WiFiChanged += OnWiFiChanged;
    }

    public void Dispose()
    {
        deviceInformation.BatteryChanged -= OnBatteryChanged;
        deviceInformation.NetworkChanged -= OnNetworkChanged;
        deviceInformation.WiFiChanged -= OnWiFiChanged;
    }

    private void OnBatteryChanged(object? sender, EventArgs args)
    {
        if (deviceInformation.Battery is { } battery)
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateBattery(battery));
        }
    }

    private void OnNetworkChanged(object? sender, EventArgs args)
    {
        if (deviceInformation.Network is { } network)
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateConnectivity(network));
        }
    }

    private void OnWiFiChanged(object? sender, EventArgs args)
    {
        var status = deviceInformation.WiFi;
        MainThread.BeginInvokeOnMainThread(() => UpdateWiFi(status));
    }

    // ------------------------------------------------------------
    // Battery
    // ------------------------------------------------------------

    private void UpdateBattery(BatteryStatus status)
    {
        log.DebugBatteryState(status.Level, status.State, status.PowerSource);

        BatteryChargeLevel = status.Level;
        BatteryState = status.State;
        BatteryPowerSource = status.PowerSource;
    }

    // ------------------------------------------------------------
    // Connectivity
    // ------------------------------------------------------------

    private void UpdateConnectivity(NetworkStatus status)
    {
        var access = status.Access;
        var profile = NetworkProfile.Unknown;
        foreach (var value in status.Profiles)
        {
            switch (value)
            {
                case ConnectionProfile.Bluetooth:
                    profile |= NetworkProfile.Bluetooth;
                    break;
                case ConnectionProfile.Cellular:
                    profile |= NetworkProfile.Cellular;
                    break;
                case ConnectionProfile.Ethernet:
                    profile |= NetworkProfile.Ethernet;
                    break;
                case ConnectionProfile.WiFi:
                    profile |= NetworkProfile.WiFi;
                    break;
            }
        }

        log.DebugConnectivityState(profile, access);

        NetworkProfile = profile;
        NetworkAccess = access;
        NetworkState = access.IsConnected()
            ? (profile.IsHighSpeed() ? NetworkState.ConnectedHighSpeed : NetworkState.Connected)
            : NetworkState.Disconnected;
    }

    private void UpdateWiFi(WiFiStatus? status)
    {
        WiFiSignalStrength = status?.SignalStrength ?? 0;
    }
}
