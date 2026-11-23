using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;

namespace Click_Test.Hooks;

/// <summary>
/// Represents the resolved theme state returned by
/// <see cref="PreferredThemeHook.UsePreferredTheme(RenderContext)"/>.
/// </summary>
/// <param name="IsDark">
/// <see langword="true"/> if the effective theme is dark; otherwise, <see langword="false"/>.
/// </param>
/// <param name="IsFollowingSystem">
/// <see langword="true"/> if no user preference is set and the theme follows the system preference.
/// </param>
/// <param name="TitleBarTheme">The title bar theme that matches the effective theme.</param>
/// <param name="ElementTheme">The XAML element theme that matches the effective theme.</param>
/// <param name="SetDark">
/// Sets an explicit user preference: <see langword="true"/> for dark, <see langword="false"/> for light.
/// Stops tracking the system preference.
/// </param>
/// <param name="FollowSystem">
/// Clears the user preference so the theme follows the system preference again.
/// </param>
public readonly record struct PreferredThemeState(
    bool IsDark,
    bool IsFollowingSystem,
    WindowTitleBarTheme TitleBarTheme,
    ElementTheme ElementTheme,
    Action<bool> SetDark,
    Action FollowSystem,
    Action ClearSaved);

public static class PreferredThemeHook
{
    const string Key = "AppTheme";

    /// <summary>
    /// Gets the preferred theme, using an explicit user choice if one is set,
    /// or the system theme otherwise.
    /// </summary>
    /// <remarks>
    /// The theme starts out following the system preference. Calling
    /// <see cref="PreferredThemeState.SetDark"/> stops tracking the system preference until
    /// <see cref="PreferredThemeState.FollowSystem"/> is called.
    /// </remarks>
    /// <param name="ctx">The render context used to read and update the theme state.</param>
    /// <returns>
    /// A <see cref="PreferredThemeState"/> with the current theme and the actions
    /// to change it.
    /// </returns>
    public static PreferredThemeState UsePreferredTheme(this RenderContext ctx, bool persist = true)
    {
        var systemDark = ctx.UseSystemDarkTheme();
        var (userDark, setUserDark, removeSaved) =
            ctx.UseLocalSetting<bool?>(Key, null, persist);
        var isDark = userDark ?? systemDark;

        return new PreferredThemeState(
            IsDark: isDark,
            IsFollowingSystem: userDark is null,
            TitleBarTheme: userDark is null
                ? WindowTitleBarTheme.UseDefaultAppMode
                : (userDark.Value ? WindowTitleBarTheme.Dark : WindowTitleBarTheme.Light),
            ElementTheme: userDark is null
                ? ElementTheme.Default
                : (userDark.Value ? ElementTheme.Dark : ElementTheme.Light),
            SetDark: v => setUserDark(v),
            FollowSystem: () => setUserDark(null),
            ClearSaved: removeSaved);
    }
}

