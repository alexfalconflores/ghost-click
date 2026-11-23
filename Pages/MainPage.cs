using Click_Test.Services;
using Microsoft.UI.Input;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using static Microsoft.UI.Reactor.Factories;

namespace Click_Test.Pages;

public enum ClickButton { Left, Right }

// ── Model ──
public record ButtonStats(int Clicks, int GhostClicks, ulong? LastTimestamp)
{
    public static readonly ButtonStats Empty = new(0, 0, null);
}

public record ClickEntry(ClickButton Button, double IntervalSec, bool IsGhost);

public record ClickState(ButtonStats Left, ButtonStats Right, ClickEntry[] History)
{
    public static readonly ClickState Initial = new(ButtonStats.Empty, ButtonStats.Empty, []);

    public bool GhostDetected => Left.GhostClicks + Right.GhostClicks > 0;
}

public class MainPage : Component
{
    // ── Update ──
    // timestamp: microsegundos del PointerPoint
    static ClickState OnClick(ClickState s, ClickButton button, ulong timestamp, int thresholdMs)
    {
        var stats = button == ClickButton.Left ? s.Left : s.Right;

        double? interval = stats.LastTimestamp is ulong last
            ? (timestamp - last) / 1_000_000.0
            : null; // primer clic de este botón: no hay intervalo

        var isGhost = interval <= thresholdMs / 1000.0;

        var updated = stats with
        {
            Clicks = stats.Clicks + 1,
            GhostClicks = stats.GhostClicks + (isGhost ? 1 : 0),
            LastTimestamp = timestamp,
        };

        ClickEntry[] history = interval is double d
            ? [new ClickEntry(button, d, isGhost), .. s.History]
            : s.History;

        return button == ClickButton.Left
            ? s with { Left = updated, History = history }
            : s with { Right = updated, History = history };
    }

    static ClickState OnReset(ClickState _) => ClickState.Initial;

    // ── View ──
    public override Element Render()
    {
        var intl = UseIntl();
        var (state, update) = UseReducer(ClickState.Initial);
        var thresholdMs = AppSettings.ThresholdMs;

        void OnPressed(PointerPoint point)
        {
            ClickButton? button = point.Properties.PointerUpdateKind switch
            {
                PointerUpdateKind.LeftButtonPressed => ClickButton.Left,
                PointerUpdateKind.RightButtonPressed => ClickButton.Right,
                _ => null, // rueda u otros botones: se ignoran
            };
            if (button is { } b)
                update(s => OnClick(s, b, point.Timestamp, thresholdMs));
        }

        string Short(ClickButton b) => intl.Message(b == ClickButton.Left ? Loc.Main.LeftShort : Loc.Main.RightShort);

        var historyText = string.Join("\n", state.History.Select(h =>
            $"{Short(h.Button)}   {intl.FormatNumber(h.IntervalSec)} s" +
            (h.IsGhost ? $"   ← {intl.Message(Loc.Main.GhostMarker)}" : "")));

        Element Stats(MessageKey label, ButtonStats stats) =>
            VStack(2,
                BodyStrong(intl.Message(label)),
                Caption($"{intl.Message(Loc.Main.Clicks)}: {intl.FormatNumber(stats.Clicks)}  ·  " +
                        $"{intl.Message(Loc.Main.DoubleClicks)}: {intl.FormatNumber(stats.GhostClicks)}"));

        return Grid(
            columns: [GridSize.Auto, GridSize.Star(), GridSize.Auto],
            rows: [GridSize.Auto, GridSize.Star(), GridSize.Star()],

            HStack(24,
                Stats(Loc.Main.LeftButton, state.Left),
                Stats(Loc.Main.RightButton, state.Right)
            ).VAlign(VerticalAlignment.Center)
             .Grid(row: 0, column: 0),

            Caption(intl.Message(Loc.Main.ThresholdInfo, ("ms", thresholdMs)))
                .Center()
                .Grid(row: 0, column: 1),

            Button(intl.Message(Loc.Main.Reset), () => update(OnReset))
                .HAlign(HorizontalAlignment.Right)
                .VAlign(VerticalAlignment.Center)
                .Grid(row: 0, column: 2),

            Border(Empty())
                .CornerRadius(4)
                .Background(state.GhostDetected ? Theme.SystemCritical : Theme.SystemCaution)
                .AutomationName(intl.Message(Loc.Main.TestArea))
                .OnPointerPressed((s, e) => OnPressed(e.GetCurrentPoint(null)))
                .Grid(row: 1, column: 0, columnSpan: 3),

            TextBox(historyText)
                .AutomationName(intl.Message(Loc.Main.Intervals))
                .Set(tb =>
                {
                    tb.IsReadOnly = true;
                    tb.AcceptsReturn = true;
                    tb.TextWrapping = TextWrapping.Wrap;
                })
                .Grid(row: 2, column: 0, columnSpan: 3)
        )
        .Set(g =>
        {
            g.ColumnSpacing = 8;
            g.RowSpacing = 16;
            g.ChildrenTransitions = [new EntranceThemeTransition { IsStaggeringEnabled = true }];
        })
        .Margin(24, 0, 24, 24);
    }
}