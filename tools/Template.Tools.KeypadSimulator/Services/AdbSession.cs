namespace Template.Tools.KeypadSimulator.Services;

using System.ComponentModel;
using System.Diagnostics;

public sealed class AdbSession : IDisposable
{
    private static readonly TimeSpan ExitTimeout = TimeSpan.FromSeconds(3);

    private readonly Process process;

    private volatile bool disposed;

    public event EventHandler? Exited;

    public AdbSession(ILogger log, string adbPath, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo(adbPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.Exited += (_, _) =>
        {
            if (!disposed)
            {
                Exited?.Invoke(this, EventArgs.Empty);
            }
        };
        process.OutputDataReceived += static (_, _) => { };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                log.WarnAdbSessionOutput(e.Data);
            }
        };

        process.Start();
        process.StandardInput.NewLine = "\n";
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        try
        {
            process.StandardInput.Close();
            if (!process.WaitForExit(ExitTimeout))
            {
                process.Kill(true);
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or Win32Exception)
        {
            Debug.WriteLine(ex);
        }

        process.Dispose();
    }

    public bool HasExited => process.HasExited;

    public bool WriteLine(string line)
    {
        if (disposed)
        {
            return false;
        }

        try
        {
            process.StandardInput.WriteLine(line);
            process.StandardInput.Flush();
            return true;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }
}
