using Click_Test.Hooks;
using Click_Test.Services;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;

namespace Click_Test;

/// Umbral de doble clic compartido entre MainPage y SettingsPage.
public readonly record struct ThresholdState(int Ms, Action<int> Set);

public static class AppContexts
{
    // Valores por defecto seguros: si algo lee el contexto fuera del
    // provider, las acciones no hacen nada en vez de lanzar NullReferenceException.
    public static readonly Context<PreferredThemeState> Theme = new(new PreferredThemeState(
        IsDark: false,
        IsFollowingSystem: true,
        TitleBarTheme: WindowTitleBarTheme.UseDefaultAppMode,
        ElementTheme: ElementTheme.Default,
        SetDark: _ => { },
        FollowSystem: () => { },
        ClearSaved: () => { }));

    public static readonly Context<ThresholdState> Threshold = new(new ThresholdState(
        AppSettings.DefaultThresholdMs, _ => { }));
}