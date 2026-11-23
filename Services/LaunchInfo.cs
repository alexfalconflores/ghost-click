using Windows.ApplicationModel;
using Windows.Storage;

namespace Click_Test.Services;

public enum StartupDialog { None, FirstRun, WhatsNew }

public static class LaunchInfo
{
    const string LastVersionKey = "LastLaunchedVersion";
    static bool consumed; // equivale a tus flags `shown`
    public static string CurrentVersion { get; } = GetVersion();


    public static bool IsFirstRun { get; }
    public static bool IsAppUpdated { get; }

    static LaunchInfo()
    {
        var values = ApplicationData.Current.LocalSettings.Values;
        var v = Package.Current.Id.Version;
        var current = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
        var last = values[LastVersionKey] as string;

        IsFirstRun = last is null;
        IsAppUpdated = last is not null && last != current;
        values[LastVersionKey] = current;
    }

    // Devuelve el diálogo pendiente una sola vez por ejecución
    public static StartupDialog TakePending()
    {
        if (consumed) return StartupDialog.None;
        consumed = true;
        return IsFirstRun ? StartupDialog.FirstRun
             : IsAppUpdated ? StartupDialog.WhatsNew
             : StartupDialog.None;
    }

    static string GetVersion()
    {
        var v = Package.Current.Id.Version;
        return $"{v.Major}.{v.Minor}.{v.Build}";
    }
}