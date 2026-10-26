namespace Template.Tools.KeypadSimulator.Views;

using Template.Tools.KeypadSimulator.Services;
using Template.Tools.KeypadSimulator.Settings;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainWindowViewModel : ExtendViewModelBase
{
    private readonly ILogger<MainWindowViewModel> log;

    private readonly UserSettings settings;

    private readonly AdbService adbService;

    private readonly SynchronizationContext? context;

    private IKeySender? keySender;

    // Property

    public IWindowManager WindowManager { get; }

    public KeypadKeyMap Keys { get; }

    public ObservableCollection<AdbDevice> Devices { get; } = [];

    [ObservableProperty]
    public partial AdbDevice? SelectedDevice { get; set; }

    [ObservableProperty]
    public partial bool IsHardware { get; set; }

    [ObservableProperty]
    public partial bool IsInput { get; set; }

    [ObservableProperty]
    public partial bool IsAlphabetic { get; set; }

    [ObservableProperty]
    public partial bool IsNumeric { get; set; }

    [ObservableProperty]
    public partial bool PcKeys { get; set; }

    [ObservableProperty]
    public partial bool Numpad { get; set; }

    [ObservableProperty]
    public partial bool Topmost { get; set; }

    [ObservableProperty]
    public partial bool IsConnected { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; }

    [ObservableProperty]
    public partial string LastKey { get; set; }

    // Command

    public IObserveCommand RefreshCommand { get; }

    public IObserveCommand ConnectCommand { get; }

    public IObserveCommand PressCommand { get; }

    public IObserveCommand ReleaseCommand { get; }

    public IObserveCommand PcKeyDownCommand { get; }

    public IObserveCommand PcKeyUpCommand { get; }

    public IObserveCommand ReleaseAllCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> log,
        IWindowManager windowManager,
        UserSettings settings,
        AdbService adbService,
        KeypadKeyMap keys)
    {
        this.log = log;
        WindowManager = windowManager;
        this.settings = settings;
        this.adbService = adbService;
        Keys = keys;
        context = SynchronizationContext.Current;

        IsHardware = settings.Mode == SendMode.Hardware;
        IsInput = !IsHardware;
        IsAlphabetic = settings.KeyboardType == KeyboardType.Alphabetic;
        IsNumeric = !IsAlphabetic;
        PcKeys = settings.PcKeys;
        Numpad = settings.Numpad;
        Topmost = settings.Topmost;
        Status = "Disconnected";
        LastKey = string.Empty;

        RefreshCommand = MakeAsyncCommand(RefreshAsync, () => !IsConnected);
        ConnectCommand = MakeAsyncCommand(ConnectAsync);
        PressCommand = MakeDelegateCommand<KeypadKey>(Press);
        ReleaseCommand = MakeDelegateCommand<KeypadKey>(Release);
        PcKeyDownCommand = MakeDelegateCommand<PcKeyInput>(PcKeyDown, _ => PcKeys && IsConnected);
        PcKeyUpCommand = MakeDelegateCommand<PcKeyInput>(PcKeyUp, _ => PcKeys && IsConnected);
        ReleaseAllCommand = MakeDelegateCommand(ReleaseAll);

        SubscribeIsHardware(x => ChangeMode(x, SendMode.Hardware));
        SubscribeIsInput(x => ChangeMode(x, SendMode.Input));
        SubscribeIsAlphabetic(x => ChangeKeyboardType(x, KeyboardType.Alphabetic));
        SubscribeIsNumeric(x => ChangeKeyboardType(x, KeyboardType.Numeric));
        SubscribePcKeys(x => settings.PcKeys = x);
        SubscribeNumpad(x => settings.Numpad = x);
        SubscribeTopmost(x => settings.Topmost = x);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CloseSender();
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Device
    //--------------------------------------------------------------------------------

    private async Task RefreshAsync()
    {
        if (adbService.AdbPath is null)
        {
            Status = "adb not found";
            return;
        }

        IReadOnlyList<AdbDevice> devices;
        try
        {
            devices = await adbService.GetDevicesAsync(CancellationToken.None);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or OperationCanceledException)
        {
            log.WarnAdbCommandFailed(ex, "devices");
            Status = "adb failed";
            return;
        }

        var serial = SelectedDevice?.Serial ?? settings.Serial;
        Devices.Clear();
        foreach (var device in devices)
        {
            Devices.Add(device);
        }

        SelectedDevice = Devices.FirstOrDefault(x => x.Serial == serial) ?? Devices.FirstOrDefault(static x => x.IsReady) ?? Devices.FirstOrDefault();
        Status = Devices.Count > 0 ? "Disconnected" : "No device";
    }

    //--------------------------------------------------------------------------------
    // Connection
    //--------------------------------------------------------------------------------

    private async Task ConnectAsync()
    {
        if (keySender is not null)
        {
            CloseSender();
            Status = "Disconnected";
            return;
        }

        if (SelectedDevice is not { IsReady: true } device)
        {
            Status = "Select a device";
            return;
        }

        if (adbService.AdbPath is null)
        {
            Status = "adb not found";
            return;
        }

        Status = "Connecting...";
        while (true)
        {
            var mode = settings.Mode;
            var keyboardType = settings.KeyboardType;
            IKeySender sender = mode == SendMode.Hardware
                ? new UinputKeySender(adbService, Keys, keyboardType)
                : new InputKeySender(adbService);
            sender.Disconnected += OnSenderDisconnected;

            bool connected;
            try
            {
                connected = await sender.ConnectAsync(device.Serial, CancellationToken.None);
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or IOException or OperationCanceledException)
            {
                log.WarnAdbCommandFailed(ex, "connect");
                connected = false;
            }

            if (connected && ((mode != settings.Mode) || (keyboardType != settings.KeyboardType)))
            {
                sender.Disconnected -= OnSenderDisconnected;
                sender.Dispose();
                continue;
            }

            if (!connected)
            {
                sender.Disconnected -= OnSenderDisconnected;
                sender.Dispose();
                log.WarnConnectFailed(device.Serial, sender.Description);
                Status = "Connection failed";
                return;
            }

            keySender = sender;
            settings.Serial = device.Serial;
            IsConnected = true;
            Status = $"Connected: {sender.Description}";
            log.InfoConnected(device.Serial, sender.Description);
            return;
        }
    }

    private void CloseSender()
    {
        if (keySender is null)
        {
            return;
        }

        keySender.Disconnected -= OnSenderDisconnected;
        keySender.Dispose();
        keySender = null;
        IsConnected = false;
        log.InfoDisconnected();
    }

    private void OnSenderDisconnected(object? sender, EventArgs e)
    {
        if (context is null)
        {
            HandleDisconnected(sender);
        }
        else
        {
            context.Post(_ => HandleDisconnected(sender), null);
        }
    }

    private void HandleDisconnected(object? sender)
    {
        if (ReferenceEquals(sender, keySender))
        {
            CloseSender();
            Status = "Device disconnected";
        }
    }

    private void ChangeMode(bool selected, SendMode mode)
    {
        if (selected && (settings.Mode != mode))
        {
            settings.Mode = mode;
            IsHardware = mode == SendMode.Hardware;
            IsInput = mode == SendMode.Input;
            Reconnect();
        }
    }

    private void ChangeKeyboardType(bool selected, KeyboardType keyboardType)
    {
        if (selected && (settings.KeyboardType != keyboardType))
        {
            settings.KeyboardType = keyboardType;
            IsAlphabetic = keyboardType == KeyboardType.Alphabetic;
            IsNumeric = keyboardType == KeyboardType.Numeric;
            if (settings.Mode == SendMode.Hardware)
            {
                Reconnect();
            }
        }
    }

    private void Reconnect()
    {
        if (keySender is not null)
        {
            CloseSender();
            ConnectCommand.Execute(null);
        }
    }

    //--------------------------------------------------------------------------------
    // Key
    //--------------------------------------------------------------------------------

    private void Press(KeypadKey key) => Send(key, Numpad);

    private void Release(KeypadKey key) => keySender?.Release(key);

    private void PcKeyDown(PcKeyInput input)
    {
        if (!input.IsRepeat || (settings.Mode != SendMode.Hardware))
        {
            Send(Keys[input.Id], Numpad || input.Numpad);
        }
    }

    private void PcKeyUp(PcKeyInput input) => keySender?.Release(Keys[input.Id]);

    private void ReleaseAll() => keySender?.ReleaseAll();

    private void Send(KeypadKey key, bool numpad)
    {
        if (keySender is null)
        {
            Status = "Not connected";
            return;
        }

        var code = keySender.Press(key, numpad);
        if (code is not null)
        {
            LastKey = $"Last: {code}";
            log.DebugKeySent(code);
        }
    }
}
