using Click_Test.Services;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Microsoft.UI.Reactor.Factories;

namespace Click_Test.Dialogs;

public class StartupDialogs : Component
{

    static Element Paragraph(string text) =>
        TextBlock(text).TextWrapping(TextWrapping.WrapWholeWords);

    static Element ListItem(string marker, string text) =>
        Grid(
            columns: [GridSize.Auto, GridSize.Star()],
            rows: [GridSize.Auto],
            TextBlock(marker).Width(20).Grid(row: 0, column: 0),
            Paragraph(text).Grid(row: 0, column: 1));

    static Element Header(string title) =>
        HStack(12,
            Image("ms-appx:///Assets/StoreLogo.png").Size(40, 40).AccessibilityHidden(),
            Subtitle(title).VAlign(VerticalAlignment.Center));

    static Element WelcomeBody(IntlAccessor intl) =>
        VStack(12,
            Paragraph(intl.Message(Loc.Onboarding.WelcomeIntro)),
            VStack(6,
                ListItem("1.", intl.Message(Loc.Onboarding.WelcomeStep1)),
                ListItem("2.", intl.Message(Loc.Onboarding.WelcomeStep2, ("threshold", AppSettings.ThresholdMs))),
                ListItem("3.", intl.Message(Loc.Onboarding.WelcomeStep3))),
            Paragraph(intl.Message(Loc.Onboarding.WelcomeOutro)));

    static Element WhatsNewBody(IntlAccessor intl) =>
        VStack(6,
            intl.Message(Loc.Onboarding.WhatsNewChanges)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => ListItem("•", line).WithKey(line))
                .ToArray());

    public override Element Render()
    {
        var intl = UseIntl();
        var (dialog, setDialog) = UseState(LaunchInfo.TakePending());

        var (title, body, button) = dialog == StartupDialog.FirstRun
            ? (intl.Message(Loc.Onboarding.WelcomeTitle), WelcomeBody(intl), intl.Message(Loc.Common.GetStarted))
            : (intl.Message(Loc.Onboarding.WhatsNewTitle, ("version", LaunchInfo.CurrentVersion)),
               WhatsNewBody(intl), intl.Message(Loc.Common.Ok));

        return ContentDialog(
            "",
            VStack(16, Header(title), ScrollView(body)),
            primaryButtonText: button) with
        {
            IsOpen = dialog != StartupDialog.None,
            DefaultButton = ContentDialogButton.Primary,
            OnClosed = _ => setDialog(StartupDialog.None),
        };
    }
}