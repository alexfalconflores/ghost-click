using Click_Test.Services;
using Microsoft.UI.Input;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using System.Collections.Immutable;
using static Microsoft.UI.Reactor.Factories;
using Microsoft.UI.Xaml.Controls;

namespace Click_Test.Pages;

public enum ClickButton { Left, Middle, Right, Back, Forward }

// ── Model ──
public record ButtonStats(int Clicks, int GhostClicks, ulong? LastTimestamp)
{
    public static readonly ButtonStats Empty = new(0, 0, null);
}

public record ClickEntry(long Id, ClickButton Button, double IntervalSec, bool IsGhost);

public record ClickState(
    ImmutableDictionary<ClickButton, ButtonStats> Stats,
    ImmutableHashSet<ClickButton> Pressed,
    ClickEntry[] History,
    long NextId)
{
    public static readonly ClickState Initial = new(
        ImmutableDictionary<ClickButton, ButtonStats>.Empty,
        ImmutableHashSet<ClickButton>.Empty,
        [],
        0);

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
            History = interval is double d
                ? [new ClickEntry(s.NextId, button, d, isGhost), .. s.History.Take(499)]
                : s.History,
            NextId = interval is null ? s.NextId : s.NextId + 1,
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

    const double PaneWidth = 268; // 260 de tarjeta + 8 de separación
    static readonly UISettings SystemUI = new();


    // Anima el ancho del panel: al crecer empuja a la zona de prueba
    static void AnimatePane(FrameworkElement pane, bool open)
    {
        var to = open ? PaneWidth : 0;
        if (pane.Tag is double current && current == to) return; // ya está (o va) hacia ahí

        var firstRender = pane.Tag is null;
        pane.Tag = to;

        // Primer render o animaciones desactivadas en Windows: sin animación
        if (firstRender || !SystemUI.AnimationsEnabled)
        {
            pane.Width = to;
            pane.Opacity = open ? 1 : 0;
            pane.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        pane.Visibility = Visibility.Visible;

        var duration = TimeSpan.FromMilliseconds(250);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var width = new DoubleAnimation { To = to, Duration = duration, EasingFunction = ease, EnableDependentAnimation = true };
        Storyboard.SetTarget(width, pane);
        Storyboard.SetTargetProperty(width, "Width");

        var fade = new DoubleAnimation { To = open ? 1 : 0, Duration = duration, EasingFunction = ease };
        Storyboard.SetTarget(fade, pane);
        Storyboard.SetTargetProperty(fade, "Opacity");

        var sb = new Storyboard();
        sb.Children.Add(width);
        sb.Children.Add(fade);
        sb.Completed += (_, _) =>
        {
            // Cerrado: fuera del árbol de accesibilidad y del foco con Tab
            if (pane.Tag is 0d) pane.Visibility = Visibility.Collapsed;
        };
        sb.Begin();
    }


    // ── View ──
    public override Element Render()
    {
        var intl = UseIntl();
        var (state, update) = UseReducer(ClickState.Initial);
        var (historyOpen, setHistoryOpen) = UseState(false);
        var thresholdMs = UseContext(AppContexts.Threshold).Ms;

        string Name(ClickButton b) => intl.Message(NameKey(b));
        string Ms(double sec) => $"{intl.FormatNumber(Math.Round(sec * 1000, 1))} ms";

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

        // ── Historial (derecha) ──
        Element HistoryRow(ClickEntry h, int i)
        {
            var time = Caption(Ms(h.IntervalSec));
            if (h.IsGhost) time = time.Foreground(Theme.SystemCritical);

            return Grid(
                columns: [GridSize.Star(), GridSize.Auto],
                rows: [GridSize.Auto],
                Caption(Name(h.Button) + (h.IsGhost ? $" · {intl.Message(Loc.Main.GhostMarker)}" : ""))
                    .Grid(row: 0, column: 0),
                time.Grid(row: 0, column: 1));
        }

        Element historyBody = state.History.Length == 0
            ? Caption(intl.Message(Loc.Main.HistoryEmpty)).Opacity(0.7).TextWrapping(TextWrapping.Wrap)
            : LazyVStack<ClickEntry>(state.History, h => h.Id.ToString(), HistoryRow);

        // Grid con fila Star: LazyVStack necesita una altura acotada para virtualizar
        var historyCard = Border(
                Grid(
                    columns: [GridSize.Star()],
                    rows: [GridSize.Auto, GridSize.Star()],
                    BodyStrong(intl.Message(Loc.Main.History)).Grid(row: 0, column: 0),
                    historyBody.Grid(row: 1, column: 0)
                ).Set(g => g.RowSpacing = 8))
            .Width(PaneWidth - 8)
            .Margin(8, 0, 0, 0)
            .HAlign(HorizontalAlignment.Left)
            .Padding(12)
            .CornerRadius(8)
            .Background(Theme.CardBackground)
            .AutomationName(intl.Message(Loc.Main.Intervals));

        // Contenedor que se anima: recorta la tarjeta mientras crece
        var historyPane = Border(historyCard)
            .Set(b => AnimatePane(b, historyOpen));

        // ── Zona de prueba ──
        var ghost = state.GhostDetected;
        var anyPressed = !state.Pressed.IsEmpty;

        var testArea = Border(
                VStack(8,
                    Icon(FontIcon(ghost ? "\uE7BA" : "\uE962", fontSize: 48)),
                    Subtitle(intl.Message(ghost ? Loc.Main.GhostTitle : Loc.Main.ClickHere)),
                    Caption(intl.Message(ghost ? Loc.Main.GhostHint : Loc.Main.ClickHereHint))
                        .Opacity(0.7)
                        .TextWrapping(TextWrapping.Wrap)
                ).Center()
                 .IsHitTestVisible(false)) // los eventos los recibe solo el Border
            .CornerRadius(8)
            .Background(anyPressed
                ? Theme.Ref("CardBackgroundFillColorSecondaryBrush")
                : Theme.CardBackground)
            .BorderThickness(ghost ? 2 : 1)
            .BorderBrush(ghost ? Theme.SystemCritical : Theme.Ref("CardStrokeColorDefaultBrush"))
            .AutomationName(intl.Message(Loc.Main.TestArea))
            .OnPointerPressed((s, e) => OnPointer(s, e))
            .OnPointerMoved((s, e) => OnPointer(s, e))
            .OnPointerReleased((s, e) => OnPointer(s, e))
            .OnPointerCaptureLost((s, e) => update(OnAllReleased));

        // ── Cabecera ──
        var historyLabel = intl.Message(historyOpen ? Loc.Main.HideHistory : Loc.Main.ShowHistory);
        var historyButton = Button(
                HStack(8,
                    Icon(FontIcon(historyOpen ? "\uE76C" : "\uE81C", fontSize: 16)),
                    TextBlock(historyLabel)),
                () => setHistoryOpen(!historyOpen))
            .AutomationName(historyLabel);

        var header = Grid(
            columns: [GridSize.Star(), GridSize.Auto],
            rows: [GridSize.Auto],
            Caption(intl.Message(Loc.Main.ThresholdInfo, ("ms", thresholdMs)))
                .VAlign(VerticalAlignment.Center)
                .Grid(row: 0, column: 0),
            HStack(8,
                historyButton,
                Button(intl.Message(Loc.Main.Reset), () => update(OnReset))
            ).Grid(row: 0, column: 1));

        return Grid(
            columns: [GridSize.Star(), GridSize.Auto],
            rows: [GridSize.Auto, GridSize.Auto, GridSize.Star()],

            header.Grid(row: 0, column: 0, columnSpan: 2),

            // Las 5 tarjetas, mismo ancho
            Grid(
                columns: [GridSize.Star(), GridSize.Star(), GridSize.Star(), GridSize.Star(), GridSize.Star()],
                rows: [GridSize.Auto],
                [.. Buttons.Select((b, i) => ButtonCard(b).Grid(row: 0, column: i).WithKey(b.ToString()))]
            ).Set(g => g.ColumnSpacing = 8)
             .Grid(row: 1, column: 0, columnSpan: 2),

            testArea.Grid(row: 2, column: 0),
            historyPane.Grid(row: 2, column: 1)
        )
        .Set(g =>
        {
            g.RowSpacing = 16; // sin ColumnSpacing: el hueco va dentro del panel para que se anime
            //g.ChildrenTransitions = [new EntranceThemeTransition { IsStaggeringEnabled = true }];
        })
        .OnMount(g => g.Transitions =
            [new EntranceThemeTransition { IsStaggeringEnabled = true }])   // una sola vez
        .Margin(24, 0, 24, 24);
    }
}