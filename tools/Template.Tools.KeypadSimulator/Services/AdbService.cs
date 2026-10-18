namespace Template.Tools.KeypadSimulator.Services;

using System.Diagnostics;

using Template.Tools.KeypadSimulator.Settings;

public sealed record AdbDevice(string Serial, string State, string Model)
{
    public bool IsReady => State == "device";

    public string Display => IsReady ? $"{Model} ({Serial})" : $"{Serial} ({State})";
}

public sealed class AdbService
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    private readonly ILogger<AdbService> log;

    private readonly SimulatorSettings settings;

    public AdbService(
        ILogger<AdbService> log,
        SimulatorSettings settings)
    {
        this.log = log;
        this.settings = settings;
    }

    //--------------------------------------------------------------------------------
    // Adb
    //--------------------------------------------------------------------------------

    public string? AdbPath => field ??= EnumerateAdbPaths().FirstOrDefault(File.Exists);

    private IEnumerable<string> EnumerateAdbPaths()
    {
        if (!String.IsNullOrEmpty(settings.AdbPath))
        {
            yield return settings.AdbPath;
        }

        foreach (var variable in (string[])["ANDROID_HOME", "ANDROID_SDK_ROOT"])
        {
            var root = Environment.GetEnvironmentVariable(variable);
            if (!String.IsNullOrEmpty(root))
            {
                yield return Path.Combine(root, "platform-tools", "adb.exe");
            }
        }

        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk", "platform-tools", "adb.exe");
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Android", "android-sdk", "platform-tools", "adb.exe");

        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return Path.Combine(directory, "adb.exe");
        }
    }

    //--------------------------------------------------------------------------------
    // Device
    //--------------------------------------------------------------------------------

    public async ValueTask<IReadOnlyList<AdbDevice>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var output = await RunAsync(["devices", "-l"], cancellationToken);

        var devices = new List<AdbDevice>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith("List of devices", StringComparison.Ordinal) || line.StartsWith('*'))
            {
                continue;
            }

            var values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (values.Length < 2)
            {
                continue;
            }

            var model = values.FirstOrDefault(static x => x.StartsWith("model:", StringComparison.Ordinal));
            devices.Add(new AdbDevice(values[0], values[1], model is not null ? model["model:".Length..].Replace('_', ' ') : values[0]));
        }

        return devices;
    }

    public async ValueTask<bool> WaitForInputDevicesAsync(string serial, IReadOnlyList<string> names, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < timeout)
        {
            var output = await RunAsync(["-s", serial, "shell", "dumpsys", "input"], cancellationToken);
            if (names.All(x => output.Contains(x, StringComparison.Ordinal)))
            {
                return true;
            }

            await Task.Delay(PollInterval, cancellationToken);
        }

        return false;
    }

    //--------------------------------------------------------------------------------
    // Session
    //--------------------------------------------------------------------------------

    public AdbSession StartShell(string serial, params string[] command) =>
        new(log, AdbPath ?? throw new InvalidOperationException("adb not found."), ["-s", serial, "shell", .. command]);

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private async ValueTask<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(AdbPath ?? throw new InvalidOperationException("adb not found."))
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            StandardOutputEncoding = Encoding.UTF8
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancel.CancelAfter(CommandTimeout);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("adb could not be started.");
        try
        {
            var output = await process.StandardOutput.ReadToEndAsync(cancel.Token);
            await process.WaitForExitAsync(cancel.Token);
            return output;
        }
        catch (OperationCanceledException ex)
        {
            log.WarnAdbCommandFailed(ex, String.Join(' ', arguments));
            process.Kill(true);
            throw;
        }
    }
}
