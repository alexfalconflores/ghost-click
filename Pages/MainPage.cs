using Click_Test.Services;
using Microsoft.UI.Input;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.Immutable;
using static Microsoft.UI.Reactor.Factories;

namespace Click_Test.Pages;

public enum ClickButton { Left, Middle, Right, Back, Forward }

// ── Model ──
public record ButtonStats(int Clicks, int GhostClicks, ulong? LastTimestamp)
{
    public static readonly ButtonStats Empty = new(0, 0, null);
}

public record ClickEntry(ClickButton Button, double IntervalSec, bool IsGhost);

public record ClickState(
    ImmutableDictionary<ClickButton, ButtonStats> Stats,
    ImmutableHashSet<ClickButton> Pressed,
    ClickEntry[] History)
{
    public static readonly ClickState Initial = new(
        ImmutableDictionary<ClickButton, ButtonStats>.Empty,
        ImmutableHashSet<ClickButton>.Empty,
        []);

    public ButtonStats StatsFor(ClickButton b) => Stats.GetValueOrDefault(b, ButtonStats.Empty);
    public bool GhostDetected => Stats.Values.Any(s => s.GhostClicks > 0);
}

public class MainPage : Component
{
    static readonly ClickButton[] Buttons = Enum.GetValues<ClickButton>();

    // ── Update ──
    // timestamp: microsegundos del PointerPoint
    static ClickState OnButtonDown(ClickState s, ClickButton button, ulong timestamp, int thresholdMs)
    {
        var stats = s.StatsFor(button);

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

        return s with
        {
            Stats = s.Stats.SetItem(button, updated),
            Pressed = s.Pressed.Add(button),
            History = interval is double d ? [new ClickEntry(button, d, isGhost), .. s.History] : s.History,
        };
    }

    static ClickState OnButtonUp(ClickState s, ClickButton button) =>
        s with { Pressed = s.Pressed.Remove(button) };

    static ClickState OnAllReleased(ClickState s) =>
        s with { Pressed = ImmutableHashSet<ClickButton>.Empty };

    static ClickState OnReset(ClickState _) => ClickState.Initial;

    // Qué botón cambió y si se pulsó (true) o se soltó (false)
    static (ClickButton Button, bool Down)? Decode(PointerUpdateKind kind) => kind switch
    {
        PointerUpdateKind.LeftButtonPressed => (ClickButton.Left, true),
        PointerUpdateKind.LeftButtonReleased => (ClickButton.Left, false),
        PointerUpdateKind.MiddleButtonPressed => (ClickButton.Middle, true),
        PointerUpdateKind.MiddleButtonReleased => (ClickButton.Middle, false),
        PointerUpdateKind.RightButtonPressed => (ClickButton.Right, true),
        PointerUpdateKind.RightButtonReleased => (ClickButton.Right, false),
        PointerUpdateKind.XButton1Pressed => (ClickButton.Back, true),
        PointerUpdateKind.XButton1Released => (ClickButton.Back, false),
        PointerUpdateKind.XButton2Pressed => (ClickButton.Forward, true),
        PointerUpdateKind.XButton2Released => (ClickButton.Forward, false),
        _ => null, // movimiento sin cambio de botón
    };

    static MessageKey NameKey(ClickButton b) => b switch
    {
        ClickButton.Left => Loc.Main.ButtonLeft,
        ClickButton.Middle => Loc.Main.ButtonMiddle,
        ClickButton.Right => Loc.Main.ButtonRight,
        ClickButton.Back => Loc.Main.ButtonBack,
        _ => Loc.Main.ButtonForward,
    };

    // ── View ──
    public override Element Render()
    {
        var intl = UseIntl();
        var (state, update) = UseReducer(ClickState.Initial);
        var thresholdMs = AppSettings.ThresholdMs;

        string Name(ClickButton b) => intl.Message(NameKey(b));

        void OnPointer(object sender, PointerRoutedEventArgs e)
        {
            var target = sender as UIElement;
            var point = e.GetCurrentPoint(target);
            if (Decode(point.Properties.PointerUpdateKind) is not { } change) return;

            if (change.Down)
            {
                target?.CapturePointer(e.Pointer);
                update(s => OnButtonDown(s, change.Button, point.Timestamp, thresholdMs));
            }
            else
            {
                update(s => OnButtonUp(s, change.Button));
            }

            e.Handled = true; // que "atrás" no navegue y el clic derecho no abra menús
        }

        // Tarjeta por botón: indicador de pulsado + contadores
        Element ButtonCard(ClickButton b)
        {
            var stats = state.StatsFor(b);
            var pressed = state.Pressed.Contains(b);

            return Border(
                VStack(4,
                    HStack(6,
                        TextBlock("●").Opacity(pressed ? 1 : 0.15).AccessibilityHidden(),
                        BodyStrong(Name(b))),
                    Caption($"{intl.Message(Loc.Main.Clicks)}: {intl.FormatNumber(stats.Clicks)}"),
                    Caption($"{intl.Message(Loc.Main.DoubleClicks)}: {intl.FormatNumber(stats.GhostClicks)}")))
                .Padding(12, 8)
                .CornerRadius(4)
                .Background(Theme.CardBackground);
        }

        var historyText = string.Join("\n", state.History.Select(h =>
            $"{Name(h.Button)}   {intl.FormatNumber(h.IntervalSec)} s" +
            (h.IsGhost ? $"   ← {intl.Message(Loc.Main.GhostMarker)}" : "")));

        return Grid(
            columns: [GridSize.Star(), GridSize.Auto],
            rows: [GridSize.Auto, GridSize.Auto, GridSize.Star(), GridSize.Star()],

            Caption(intl.Message(Loc.Main.ThresholdInfo, ("ms", thresholdMs)))
                .VAlign(VerticalAlignment.Center)
                .Grid(row: 0, column: 0),

            Button(intl.Message(Loc.Main.Reset), () => update(OnReset))
                .HAlign(HorizontalAlignment.Right)
                .Grid(row: 0, column: 1),

            // Las 5 tarjetas, mismo ancho
            Grid(
                columns: [GridSize.Star(), GridSize.Star(), GridSize.Star(), GridSize.Star(), GridSize.Star()],
                rows: [GridSize.Auto],
                Buttons.Select((b, i) => ButtonCard(b).Grid(row: 0, column: i)).ToArray()
            ).Set(g => g.ColumnSpacing = 8)
             .Grid(row: 1, column: 0, columnSpan: 2),

            Border(Empty())
                .CornerRadius(4)
                .Background(state.GhostDetected ? Theme.SystemCritical : Theme.SystemCaution)
                .AutomationName(intl.Message(Loc.Main.TestArea))
                .OnPointerPressed((s, e) => OnPointer(s, e))
                .OnPointerMoved((s, e) => OnPointer(s, e))
                .OnPointerReleased((s, e) => OnPointer(s, e))
                .OnPointerCaptureLost((s, e) => update(OnAllReleased))
                .Grid(row: 2, column: 0, columnSpan: 2),

            TextBox(historyText)
                .AutomationName(intl.Message(Loc.Main.Intervals))
                .Set(tb =>
                {
                    tb.IsReadOnly = true;
                    tb.AcceptsReturn = true;
                    tb.TextWrapping = TextWrapping.Wrap;
                })
                .Grid(row: 3, column: 0, columnSpan: 2)
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