namespace Template.Tools.KeypadSimulator.Views;

using Microsoft.Extensions.DependencyInjection;

using Template.Tools.KeypadSimulator.Settings;

public interface IWindowManager
{
    Window Load();

    void Save();
}

public sealed class WindowManager : NotificationObject, IWindowManager
{
    private readonly IServiceProvider provider;

    private readonly UserSettings settings;

    public WindowManager(
        IServiceProvider provider,
        UserSettings settings)
    {
        this.provider = provider;
        this.settings = settings;
    }

    public Window Load()
    {
        var mainWindow = provider.GetRequiredService<MainWindow>();

        if (settings.MainWindowPlacement is not null)
        {
            RestorePlacement(mainWindow, settings.MainWindowPlacement);
        }

        mainWindow.Show();

        return mainWindow;
    }

    private static void RestorePlacement(Window window, MainWindowPlacement placement)
    {
        var left = SystemParameters.VirtualScreenLeft;
        var top = SystemParameters.VirtualScreenTop;

        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = Math.Clamp(placement.Left, left, Math.Max(left, left + SystemParameters.VirtualScreenWidth - window.Width));
        window.Top = Math.Clamp(placement.Top, top, Math.Max(top, top + SystemParameters.VirtualScreenHeight - 200));
    }

    public void Save()
    {
        var mainWindow = provider.GetRequiredService<MainWindow>();

        settings.MainWindowPlacement = new MainWindowPlacement
        {
            Left = (int)mainWindow.Left,
            Top = (int)mainWindow.Top
        };

        settings.Save();
    }
}
