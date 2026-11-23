using Windows.Foundation.Collections;
using Windows.Storage;

namespace Click_Test.Services;

public static class AppSettings
{
    const string ThresholdKey = "GhostClickThresholdMs";

    public const int DefaultThresholdMs = 80;
    public const int MinThresholdMs = 20;
    public const int MaxThresholdMs = 500;

    static IPropertySet Values => ApplicationData.Current.LocalSettings.Values;

    public static int ThresholdMs
    {
        get => Values[ThresholdKey] is int ms ? ms : DefaultThresholdMs;
        set => Values[ThresholdKey] = Math.Clamp(value, MinThresholdMs, MaxThresholdMs);
    }
}