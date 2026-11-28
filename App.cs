using Click_Test;
using Click_Test.Hooks;
using Click_Test.Services;
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
        // Valor inicial = preferencia guardada: el primer frame ya sale con el tema correcto
        var initialTheme = UseMemo(() => PreferredThemeHook.ReadSavedElementTheme(), []);
        var (elementTheme, setElementTheme) = UseState(initialTheme);
        var (thresholdMs, setThresholdMs) = UseState(AppSettings.ThresholdMs);
        var threshold = new ThresholdState(thresholdMs, ms =>
        {
            AppSettings.ThresholdMs = ms;
            setThresholdMs(ms);
        });

        var shell = Component<Shell, ShellProps>(new(
               Title: "Ghost Click",
               OnThemeChanged: setElementTheme))
           .Provide(Click_Test.AppContexts.Threshold, threshold);

        return Grid(
                columns: [GridSize.Star()],
                rows: [GridSize.Star()],
                LocaleProvider(Locale, shell,
                    resourceProvider: new MrtResourceProvider(),
                    defaultLocale: "en-US"))
            .RequestedTheme(elementTheme)
            .Backdrop(BackdropKind.Mica);

    }

    //public override Element Render() => RenderEachTime(ctx =>
    //{
    //    // El tema se resuelve en la raíz: RequestedTheme correcto desde el primer frame
    //    var theme = ctx.UsePreferredTheme(persist: true);

    //    // El umbral vive aquí para que MainPage y SettingsPage lean el mismo valor
    //    var (thresholdMs, setThresholdMs) = ctx.UseState(AppSettings.ThresholdMs);
    //    var threshold = new ThresholdState(thresholdMs, ms =>
    //    {
    //        AppSettings.ThresholdMs = ms;
    //        setThresholdMs(ms);
    //    });

    //    var shell = Component<Shell, ShellProps>(new(Title: "Ghost Click"))
    //        .Provide(Click_Test.AppContexts.Theme, theme)
    //        .Provide(Click_Test.AppContexts.Threshold, threshold);

    //    var elementTheme = theme.ElementTheme;

    //    return Grid(
    //            columns: [GridSize.Star()],
    //            rows: [GridSize.Star()],
    //            LocaleProvider(Locale, shell,
    //                resourceProvider: new MrtResourceProvider(),
    //                defaultLocale: "en-US"))
    //        .RequestedTheme(theme.ElementTheme)
    //        .Set(g =>
    //        {
    //            if (g.XamlRoot?.Content is FrameworkElement root && root.RequestedTheme != elementTheme)
    //            {
    //                root.RequestedTheme = elementTheme;
    //            }
    //        })
    //        .Backdrop(BackdropKind.Mica);
    //});

}
