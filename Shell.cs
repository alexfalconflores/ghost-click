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

public record ShellCallbacks(Action<ElementTheme> OnThemeChanged);


public record ShellProps(
    string Title,
    Callbacks<ShellCallbacks> Cb,
    bool PersistTheme = true);

public class Shell : Component<ShellProps>
{
    public override Element Render()
    {
        var titleBarRef = this.UseElementRef<TitleBar>();
        var nav = UseNavigation(AppRoute.Main);

        var settingsGlyph = UseMemo(() => new AnimatedSettingsVisualSource());


        return Memo(ctx =>
        {
            const WindowTitleBarHeight barHeight = WindowTitleBarHeight.Tall;
            ctx.UseTitleBarLayout(titleBarRef, barHeight);
            var theme = ctx.UsePreferredTheme(Props.PersistTheme);

            ctx.UseEffect(() =>
            {
                Props.Cb.Value.OnThemeChanged(theme.ElementTheme);
                return () => { };
            }, theme.ElementTheme);

            var onSettings = nav.CurrentRoute == AppRoute.Settings;
            var iconState = onSettings ? "Pressed" : "Normal";

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
                        if (onSettings) return;   // ya estás en Configuración: no hacer nada
                        nav.Navigate(AppRoute.Settings,
                            new NavigateOptions { Transition = NavigationTransition.Entrance() });
                    }
                )
                .VerticalAlignment(VerticalAlignment.Stretch)
                .VerticalContentAlignment(VerticalAlignment.Center)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Size(40, 40)
                .Margin(2)
                .ToolTip("Configuración")
                .AutomationName("Configuración");


            var titleBar = TitleBar("Ghost Click")
            .Ref(titleBarRef)
            .HeightOption(barHeight)
            .PreferredTheme(theme.TitleBarTheme)
            .WithNavigation(nav)
            .AutoRefreshDragRegions(true)
            .RightHeader(settingsButton)
            .Flex(shrink: 0);

            var pages = NavigationHost(nav, route => route switch
            {
                AppRoute.Settings => Component<SettingsPage>(),
                AppRoute.Main => Component<MainPage>(),
                _ => Component<MainPage>()
            }).Provide(AppContexts.Theme, theme);

            return FlexColumn(titleBar, Border(pages).Flex(grow: 1, basis: 0), Component<StartupDialogs>());
        }, Props, nav.CurrentRoute, nav.Depth);
    }
}