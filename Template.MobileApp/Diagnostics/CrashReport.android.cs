namespace Template.MobileApp.Diagnostics;

using Android.Runtime;

using Template.MobileApp.Helpers;

public static partial class CrashReport
{
    private static partial void PlatformStart()
    {
        AndroidEnvironment.UnhandledExceptionRaiser += static (_, args) => LogException(args.Exception);
    }

    private static partial string ResolveCrashPath() =>
        Path.Combine(AndroidHelper.GetExternalFilesDir(), "crash.json");
}
