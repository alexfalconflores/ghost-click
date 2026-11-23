using Click_Test;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Localization;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Globalization;
using static Microsoft.UI.Reactor.Factories;


// To learn more about Reactor, the Reactor project structure, and more about
// our project templates, see: https://github.com/microsoft/microsoft-ui-reactor
ReactorApp.Run<App>("Ghost Click", configure: app =>
{

    if (app.Window.AppWindow.Presenter is OverlappedPresenter presenter)
    {
        presenter.PreferredMinimumWidth = 516;
        presenter.PreferredMinimumHeight = 516;
    }
});

class App : Component
{
    // Todo el español usa "es". Cualquier otro idioma cae en inglés
    static readonly string Locale =
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es" ? "es" : "en-US";

    public override Element Render()
    {
        var (elementTheme, setElementTheme) = UseState(ElementTheme.Default);

        var shell = Component<Shell, ShellProps>(new(
            Title: "Ghost Click",
            Cb: new ShellCallbacks(OnThemeChanged: setElementTheme),
            PersistTheme: true));

        return Grid(
                columns: [GridSize.Star()],
                rows: [GridSize.Star()],
                LocaleProvider(Locale, shell,
                    resourceProvider: new MrtResourceProvider(),
                    defaultLocale: "en-US"))
            .RequestedTheme(elementTheme)
            .Backdrop(BackdropKind.Mica);
    }
}
