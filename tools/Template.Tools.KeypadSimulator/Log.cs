namespace Template.Tools.KeypadSimulator;

internal static partial class Log
{
    // Startup

    [LoggerMessage(Level = LogLevel.Information, Message = "Application start.")]
    public static partial void InfoStartup(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Runtime: os=[{osDescription}], framework=[{frameworkDescription}], rid=[{runtimeIdentifier}]")]
    public static partial void InfoStartupSettingsRuntime(this ILogger logger, string osDescription, string frameworkDescription, string runtimeIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "Application: application=[{application}], version=[{version}]")]
    public static partial void InfoStartupApplication(this ILogger logger, string application, Version? version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Environment: environment=[{environment}], contentRoot=[{contentRoot}]")]
    public static partial void InfoStartupEnvironment(this ILogger logger, string environment, string contentRoot);

    // Error

    [LoggerMessage(Level = LogLevel.Error, Message = "Unknown exception.")]
    public static partial void ErrorUnknownException(this ILogger logger, Exception ex);

    // Adb

    [LoggerMessage(Level = LogLevel.Warning, Message = "Adb command failed. arguments=[{arguments}]")]
    public static partial void WarnAdbCommandFailed(this ILogger logger, Exception ex, string arguments);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Adb session output. line=[{line}]")]
    public static partial void WarnAdbSessionOutput(this ILogger logger, string line);

    // Session

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected. serial=[{serial}], mode=[{mode}]")]
    public static partial void InfoConnected(this ILogger logger, string serial, string mode);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Connect failed. serial=[{serial}], mode=[{mode}]")]
    public static partial void WarnConnectFailed(this ILogger logger, string serial, string mode);

    [LoggerMessage(Level = LogLevel.Information, Message = "Disconnected.")]
    public static partial void InfoDisconnected(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Key sent. code=[{code}]")]
    public static partial void DebugKeySent(this ILogger logger, string code);
}
