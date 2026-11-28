using Click_Test.Dialogs;
using Click_Test.Hooks;
using Click_Test.Pages;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Hooks;
using Microsoft.UI.Reactor.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
using static Microsoft.UI.Reactor.Factories;
using XamlAnimatedIcon = Microsoft.UI.Xaml.Controls.AnimatedIcon;

namespace Click_Test;

public record ShellProps(string Title, Action<ElementTheme> OnThemeChanged);

//public record ShellCallbacks(Action<ElementTheme> OnThemeChanged);

public class Shell : Component<ShellProps>
{
    public override Element Render()
    {
        const WindowTitleBarHeight barHeight = WindowTitleBarHeight.Tall;
        var titleBarRef = this.UseElementRef<TitleBar>();
        var nav = UseNavigation(AppRoute.Main);
        var settingsGlyph = UseMemo(() => new AnimatedSettingsVisualSource(), []);

        return RenderEachTime(ctx =>
        {
            // Limitación conocida: solo se aplica al primer montaje (titleBarRef es estable).
            ctx.UseTitleBarLayout(titleBarRef, barHeight);
            var onSettings = nav.CurrentRoute == AppRoute.Settings;
            var iconState = onSettings ? "Pressed" : "Normal";
            var theme = ctx.UsePreferredTheme(persist: true);

            ctx.UseEffect(() =>
            {
                Props.OnThemeChanged(theme.ElementTheme);
                return () => { };
            }, theme.ElementTheme);


            var settingsButton =
            Button(
                AnimatedIcon(settingsGlyph)
                    .Size(16, 16)
                    .IsHitTestVisible(false)
                    .Set(icon =>
                    {
                        if (XamlAnimatedIcon.GetState(icon) != iconState)
                            XamlAnimatedIcon.SetState(icon, iconState);
                    }),
                () =>
                {
                    if (onSettings) return;   // ya estás en Configuración
                    nav.Navigate(AppRoute.Settings);
                })
            .VerticalContentAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Size(40, 40)
            .Margin(2)
            .ToolTip("Configuración")
            .AutomationName("Configuración");


            var titleBar = TitleBar(Props.Title)
                .Ref(titleBarRef)
                .HeightOption(barHeight)
                .PreferredTheme(theme.TitleBarTheme)
                .WithNavigation(nav)
                .AutoRefreshDragRegions(true)
                .RightHeader(settingsButton)
                .Flex(shrink: 0);

            // Fade en el host: las páginas ya hacen su propia entrada escalonada
            var pages = NavigationHost(nav, route => route switch
            {
                AppRoute.Main => Component<MainPage>(),
                AppRoute.Settings => Component<SettingsPage>(),
                _ => TextBlock("Página no encontrada").Padding(24)
            }) with
            { Transition = NavigationTransition.Fade() };


            return FlexColumn(
                titleBar,
                Border(pages.Provide(AppContexts.Theme, theme)).Flex(grow: 1, basis: 0),
                Component<StartupDialogs>());
        });
    }
}