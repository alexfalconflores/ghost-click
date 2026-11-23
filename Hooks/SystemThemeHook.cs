using Microsoft.UI.Reactor.Core;
using Windows.UI.ViewManagement;

namespace Click_Test.Hooks;

public static class SystemThemeHook
{
    static bool ReadIsDark(UISettings s)
    {
        // En modo oscuro, el color de fondo del sistema es negro.
        var bg = s.GetColorValue(UIColorType.Background);
        return bg.R < 128;
    }

    public static bool UseSystemDarkTheme(this RenderContext ctx)
    {
        // Una sola instancia por componente. Si no se guarda, el GC puede
        // eliminarla y el evento deja de llegar.
        var settings = ctx.UseMemo(() => new UISettings(), []);
        var (isDark, setIsDark) = ctx.UseState(ReadIsDark(settings));

        ctx.UseEffect(() =>
        {
            void OnChanged(UISettings s, object _) => setIsDark(ReadIsDark(s));
            settings.ColorValuesChanged += OnChanged;
            return () => settings.ColorValuesChanged -= OnChanged;
        }, []);

        return isDark;
    }
}

