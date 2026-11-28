using Click_Test.Controls;
using Click_Test.Services;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using static Click_Test.Controls.SettingsCardElement;
using static Click_Test.Controls.SettingsExpanderElement;
using static Microsoft.UI.Reactor.Factories;

namespace Click_Test.Pages;

public class SettingsPage : Component
{
    // Nombres propios: no se traducen
    static readonly (string Name, string Url)[] Libraries =
    [
        ("Microsoft.UI.Reactor", "https://github.com/microsoft/microsoft-ui-reactor"),
        ("CommunityToolkit.WinUI.Controls.SettingsControls", "https://github.com/CommunityToolkit/Windows"),
        ("Microsoft.WindowsAppSDK", "https://github.com/microsoft/WindowsAppSDK"),
    ];

    private static HyperlinkButtonElement Link(string text, string uri) => HyperlinkButton(text, new Uri(uri));

    // Equivale a Style="{ThemeResource ...}" del XAML.
    static TextBlockElement Styled(string text, string styleKey) =>
        TextBlock(text).Set(tb => tb.Style = (Style)Application.Current.Resources[styleKey]);

    // SettingsCard sin header y con el contenido a la izquierda (ContentAlignment="Left").
    static SettingsCardElement LeftCard(Element content) =>
        SettingsCard(header: Empty(), content: content)
            .Set(c => c.ContentAlignment = ContentAlignment.Left);

    static TransitionCollection Reposition() => [new RepositionThemeTransition()];

    public override Element Render()
    {
        var intl = UseIntl();
        var theme = UseContext(AppContexts.Theme);
        var threshold = UseContext(AppContexts.Threshold);


        void OnThresholdChanged(double value)
        {
            if (double.IsNaN(value)) return; // el usuario vació el campo
            var ms = Math.Clamp((int)Math.Round(value), AppSettings.MinThresholdMs, AppSettings.MaxThresholdMs);
            threshold.Set(ms);   // guarda en AppSettings y avisa a MainPage
        }


        var info = UseMemo(() =>
        {
            var pkg = Package.Current;
            var v = pkg.Id.Version;
            return (
                Name: pkg.DisplayName,
                Version: $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}",
                Arch: RuntimeInformation.ProcessArchitecture.ToString(),
                pkg.Id.FamilyName,
                Publisher: pkg.PublisherDisplayName);
        }, []);

        // Se construye en cada render para que siga el idioma activo
        string[] themeOptions =
        [
            intl.Message(Loc.Settings.ThemeSystem),
            intl.Message(Loc.Settings.ThemeLight),
            intl.Message(Loc.Settings.ThemeDark),
        ];

        var selected = theme.IsFollowingSystem ? 0 : (theme.IsDark ? 2 : 1);

        void OnThemeSelected(int index)
        {
            switch (index)
            {
                case 0: theme.FollowSystem(); break;
                case 1: theme.SetDark(false); break;
                case 2: theme.SetDark(true); break;
            }
        }

        return ScrollViewer(
            VStack(4, [
                Styled(intl.Message(Loc.Settings.Title), "TitleTextBlockStyle"),

                // ── Apariencia ──
                Styled(intl.Message(Loc.Settings.Appearance), "NavigationViewItemHeaderTextStyle").Margin(0, 24, 0, 0),
                SettingsCard(
                    header: intl.Message(Loc.Settings.Theme),
                    description: intl.Message(Loc.Settings.ThemeDescription),
                    content: ComboBox(themeOptions, selected, OnThemeSelected)
                        .MinWidth(160)),

                // ── Detección ──
                Styled(intl.Message(Loc.Settings.Detection), "NavigationViewItemHeaderTextStyle").Margin(0, 24, 0, 0),
                SettingsCard(
                    header: intl.Message(Loc.Settings.Threshold),
                    description: intl.Message(Loc.Settings.ThresholdDescription, ("default", AppSettings.DefaultThresholdMs)),
                    content: NumberBox(threshold.Ms, OnThresholdChanged)
                        .Range(AppSettings.MinThresholdMs, AppSettings.MaxThresholdMs)
                        .SpinButtons()
                        .AutomationName(intl.Message(Loc.Settings.Threshold))
                        . Set(nb => { nb.SmallChange = 5; nb.LargeChange = 20; })
                        .MinWidth(160)),

                // ── Acerca de ──
                Styled(intl.Message(Loc.Settings. About), "NavigationViewItemHeaderTextStyle")
                    .Margin(0, 24, 0, 0)
                    .OnMount(tb => tb. Transitions = Reposition()),

                SettingsExpander(
                    header: info.Name,
                    description: "Alex Falcon Flores",
                    headerIcon: Icon(ImageIcon(new Uri("ms-appx:///Assets/AppIcon.ico")))
                        .Size(20, 20)
                        .AccessibilityHidden(),
                    content: TextBlock(info.Version).Opacity(0.7),
                    items: (object[])[
                        LeftCard(VStack(0, [
                            HStack(8, [
                                Icon(FontIcon("\uE950", fontSize: 14)).Margin(12, 0, 0, 0),
                                TextBlock(info.Arch)
                            ]).Margin(0, 0, 0, 8),
                            Link("X / Twitter", "https://x.com/falcon_stefano"),
                            Link("LinkedIn", "https://www.linkedin.com/in/alexfalconflores/"),
                            Link(intl.Message(Loc.Settings.SendFeedback), "https://github.com/alexfalconflores/ghost-click/issues"),
                            Link(intl.Message(Loc.Settings.RateApp), $"ms-windows-store://review/?PFN={info.FamilyName}"),
                            Link(intl.Message(Loc.Settings.MoreApps), $"ms-windows-store://publisher/?name={Uri.EscapeDataString(info.Publisher)}")
                        ]))
                    ])
                    .Margin(0, 4, 0, 0)
                    .OnMount(e => e.Transitions = Reposition()),

                // ── Bibliotecas ──
                SettingsExpander(
                    header: intl.Message(Loc.Settings.Libraries),
                    headerIcon: Icon(FontIcon("\uEA86")),
                    items: (object[])[
                        LeftCard(VStack(0, [
                            TextBlock(intl.Message(Loc.Settings.LibrariesIntro))   // ← sin el espacio
                                .TextWrapping(TextWrapping.Wrap)
                                .Margin(0, 0, 0, 10),
                            .. Libraries.Select(l => Link(l.Name, l.Url))
                        ]))
                    ])
                    .OnMount(e => e.Transitions = Reposition())
            ])
            .Padding(36, 0, 36, 36)
            .OnMount(sp => sp.Transitions =
                [new EntranceThemeTransition { IsStaggeringEnabled = true }]));
    }
}