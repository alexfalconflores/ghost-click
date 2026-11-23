using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Click_Test.Hooks;

/// Ajusta el PART_LayoutRoot de un TitleBar: altura según WindowTitleBarHeight
/// y, opcionalmente, padding derecho. Los vuelve a aplicar si WinUI los cambia.
public static class TitleBarLayoutFix
{
    const string PartName = "PART_LayoutRoot";

    public static double? ToDips(WindowTitleBarHeight height) => height switch
    {
        WindowTitleBarHeight.Standard => 32,
        WindowTitleBarHeight.Tall => 48,
        _ => null
    };

    /// Aplica los ajustes y devuelve la función de limpieza.
    /// rightPadding en DIP (píxeles efectivos); null = no tocar el padding.
    public static Action Apply(
        TitleBar tb,
        WindowTitleBarHeight height = WindowTitleBarHeight.Tall,
        double? rightPadding = null)
    {
        var dips = ToDips(height);
        if (dips is null && rightPadding is null) return () => { };

        Grid? root = null;
        long heightToken = 0, paddingToken = 0;

        void EnforceHeight(DependencyObject d, DependencyProperty p)
        {
            if (d is Grid g && dips is double h && g.Height != h)
                g.Height = h;
        }

        void EnforcePadding(DependencyObject d, DependencyProperty p)
        {
            if (d is Grid g && rightPadding is double r && g.Padding.Right != r)
            {
                var pad = g.Padding;
                g.Padding = new Thickness(pad.Left, pad.Top, r, pad.Bottom);
            }
        }

        void Attach()
        {
            tb.ApplyTemplate();
            root = FindChild<Grid>(tb, PartName);
            if (root is null) return;

            if (dips is not null)
            {
                EnforceHeight(root, FrameworkElement.HeightProperty);
                heightToken = root.RegisterPropertyChangedCallback(
                    FrameworkElement.HeightProperty, EnforceHeight);
            }

            if (rightPadding is not null)
            {
                EnforcePadding(root, Grid.PaddingProperty);
                paddingToken = root.RegisterPropertyChangedCallback(
                    Grid.PaddingProperty, EnforcePadding);
            }
        }

        void OnLoaded(object s, RoutedEventArgs e)
        {
            if (root is null) Attach();
        }

        Attach();
        tb.Loaded += OnLoaded;

        return () =>
        {
            tb.Loaded -= OnLoaded;
            if (root is null) return;
            if (dips is not null)
                root.UnregisterPropertyChangedCallback(FrameworkElement.HeightProperty, heightToken);
            if (rightPadding is not null)
                root.UnregisterPropertyChangedCallback(Grid.PaddingProperty, paddingToken);
        };
    }

    static T? FindChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T fe && fe.Name == name) return fe;
            if (FindChild<T>(child, name) is { } found) return found;
        }
        return null;
    }
}

public static class TitleBarLayoutHook
{
    public static void UseTitleBarLayout(
        this RenderContext ctx,
        ElementRef<TitleBar> titleBarRef,
        WindowTitleBarHeight height = WindowTitleBarHeight.Tall,
        double? rightPadding = null)
    {
        ctx.UseEffect(() =>
            titleBarRef.Current is { } tb
                ? TitleBarLayoutFix.Apply(tb, height, rightPadding)
                : () => { },
            titleBarRef, height, rightPadding ?? -1);
    }
}