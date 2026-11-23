using Microsoft.UI.Reactor.Localization;

namespace Click_Test;

// Claves de los .resw en Strings/<idioma>/.
// Al añadir una clave a un .resw, añádela también aquí.
public static class Loc
{
    public static class Common
    {
        const string Ns = "Common";
        public static readonly MessageKey AppName = new(Ns, "AppName");
        public static readonly MessageKey Ok = new(Ns, "Ok");
        public static readonly MessageKey GetStarted = new(Ns, "GetStarted");
    }

    public static class Main
    {
        const string Ns = "Main";
        public static readonly MessageKey Clicks = new(Ns, "Clicks");
        public static readonly MessageKey DoubleClicks = new(Ns, "DoubleClicks");
        public static readonly MessageKey Reset = new(Ns, "Reset");
        public static readonly MessageKey TestArea = new(Ns, "TestArea");
        public static readonly MessageKey Intervals = new(Ns, "Intervals");
        public static readonly MessageKey ButtonLeft = new(Ns, "ButtonLeft");
        public static readonly MessageKey ButtonMiddle = new(Ns, "ButtonMiddle");
        public static readonly MessageKey ButtonRight = new(Ns, "ButtonRight");
        public static readonly MessageKey ButtonBack = new(Ns, "ButtonBack");
        public static readonly MessageKey ButtonForward = new(Ns, "ButtonForward");
        public static readonly MessageKey GhostMarker = new(Ns, "GhostMarker");
        public static readonly MessageKey ThresholdInfo = new(Ns, "ThresholdInfo");
        public static readonly MessageKey ClickHere = new(Ns, "ClickHere");
        public static readonly MessageKey ClickHereHint = new(Ns, "ClickHereHint");
        public static readonly MessageKey GhostTitle = new(Ns, "GhostTitle");
        public static readonly MessageKey GhostHint = new(Ns, "GhostHint");
        public static readonly MessageKey History = new(Ns, "History");
        public static readonly MessageKey HistoryEmpty = new(Ns, "HistoryEmpty");
        public static readonly MessageKey ShowHistory = new(Ns, "ShowHistory");
        public static readonly MessageKey HideHistory = new(Ns, "HideHistory");
    }

    public static class Settings
    {
        const string Ns = "Settings";
        public static readonly MessageKey Title = new(Ns, "Title");
        public static readonly MessageKey Appearance = new(Ns, "Appearance");
        public static readonly MessageKey Theme = new(Ns, "Theme");
        public static readonly MessageKey ThemeDescription = new(Ns, "ThemeDescription");
        public static readonly MessageKey ThemeSystem = new(Ns, "ThemeSystem");
        public static readonly MessageKey ThemeLight = new(Ns, "ThemeLight");
        public static readonly MessageKey ThemeDark = new(Ns, "ThemeDark");
        public static readonly MessageKey About = new(Ns, "About");
        public static readonly MessageKey Libraries = new(Ns, "Libraries");
        public static readonly MessageKey LibrariesIntro = new(Ns, "LibrariesIntro");
        public static readonly MessageKey SendFeedback = new(Ns, "SendFeedback");
        public static readonly MessageKey RateApp = new(Ns, "RateApp");
        public static readonly MessageKey MoreApps = new(Ns, "MoreApps");
        public static readonly MessageKey Detection = new(Ns, "Detection");
        public static readonly MessageKey Threshold = new(Ns, "Threshold");
        public static readonly MessageKey ThresholdDescription = new(Ns, "ThresholdDescription");
    }

    public static class Onboarding
    {
        const string Ns = "Onboarding";
        public static readonly MessageKey WelcomeTitle = new(Ns, "WelcomeTitle");
        public static readonly MessageKey WelcomeIntro = new(Ns, "WelcomeIntro");
        public static readonly MessageKey WelcomeStep1 = new(Ns, "WelcomeStep1");
        public static readonly MessageKey WelcomeStep2 = new(Ns, "WelcomeStep2");
        public static readonly MessageKey WelcomeStep3 = new(Ns, "WelcomeStep3");
        public static readonly MessageKey WelcomeOutro = new(Ns, "WelcomeOutro");
        public static readonly MessageKey WhatsNewTitle = new(Ns, "WhatsNewTitle");
        public static readonly MessageKey WhatsNewChanges = new(Ns, "WhatsNewChanges");
    }
}