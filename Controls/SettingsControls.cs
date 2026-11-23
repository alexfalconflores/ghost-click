using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Core.V1Protocol.Descriptor;
using Microsoft.UI.Reactor.Wrappers;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;

namespace Click_Test.Controls;

[GenerateReactorWrapper(typeof(CommunityToolkit.WinUI.Controls.SettingsCard),
    Exclude = new[] { "CommandParameter" })]
[WrapElementSlot("HeaderIcon")]
[WrapElementSlot("Description")]
[WrapManual("Header")]
[WrapManual("HeaderText")]
public partial record SettingsCardElement
{
    public Element? Header { get; init; }
    public string? HeaderText { get; init; }

    public static SettingsCardElement SettingsCard(
        Element header, Element? description = null, Element? content = null,
        Element? headerIcon = null, Optional<bool> isClickEnabled = default, Action? onClick = null) =>
        SettingsCardElement.SettingsCard(
            description: description, content: content, headerIcon: headerIcon,
            isClickEnabled: isClickEnabled, onClick: onClick) with
        { Header = header };

    public static SettingsCardElement SettingsCard(
        string header, string? description = null, Element? content = null,
        Element? headerIcon = null, Optional<bool> isClickEnabled = default, Action? onClick = null) =>
        SettingsCardElement.SettingsCard(
            description: description is null ? null : TextBlock(description),
            content: content,
            headerIcon: headerIcon, isClickEnabled: isClickEnabled, onClick: onClick)
            with
        { HeaderText = header };

    private static partial ControlDescriptor<SettingsCardElement, CommunityToolkit.WinUI.Controls.SettingsCard>
        Customize(ControlDescriptor<SettingsCardElement, CommunityToolkit.WinUI.Controls.SettingsCard> d) =>
        d.WithSettingsHeader(static e => e.Header, static e => e.HeaderText,
            static () => CommunityToolkit.WinUI.Controls.SettingsCard.HeaderProperty);
}

// A settings group that expands to reveal child SettingsCards (its Items).
[GenerateReactorWrapper(typeof(CommunityToolkit.WinUI.Controls.SettingsExpander))]
[WrapManual("Header")]
[WrapElementSlot("Description")]
[WrapElementSlot("Content")]
[WrapElementSlot("HeaderIcon")]
[WrapControlled("IsExpanded", Events = new[] { "Expanded", "Collapsed" })]
[WrapManual("HeaderText")]
public partial record SettingsExpanderElement
{
    public Element? Header { get; init; }
    public string? HeaderText { get; init; }

    public static SettingsExpanderElement SettingsExpander(
        Element header, Element? description = null, Element? content = null,
        Element? headerIcon = null, Optional<bool> isExpanded = default,
        Action<bool>? onIsExpandedChanged = null, IReadOnlyList<object>? items = null) =>
        SettingsExpanderElement.SettingsExpander(
            description: description, content: content, headerIcon: headerIcon,
            isExpanded: isExpanded, onIsExpandedChanged: onIsExpandedChanged,
            items: items?.ToArray() ?? []) with
        { Header = header };

    public static SettingsExpanderElement SettingsExpander(
        string header, string? description = null, Element? content = null,
        Element? headerIcon = null, Optional<bool> isExpanded = default,
        Action<bool>? onIsExpandedChanged = null, IReadOnlyList<object>? items = null) =>
        SettingsExpanderElement.SettingsExpander(
            description: description is null ? null : TextBlock(description),
            content: content,
            headerIcon: headerIcon, isExpanded: isExpanded,
            onIsExpandedChanged: onIsExpandedChanged, items: items?.ToArray() ?? [])
            with
        { HeaderText = header };

    private static partial ControlDescriptor<SettingsExpanderElement, CommunityToolkit.WinUI.Controls.SettingsExpander>
        Customize(ControlDescriptor<SettingsExpanderElement, CommunityToolkit.WinUI.Controls.SettingsExpander> d) =>
        d.WithSettingsHeader(static e => e.Header, static e => e.HeaderText,
            static () => CommunityToolkit.WinUI.Controls.SettingsExpander.HeaderProperty);
}

internal static class SettingsHeaderBridge
{
    private sealed class HeaderState
    {
        internal UIElement? Child;
        internal object? Value;
    }

    // Keep ownership on the native control even when Header is replaced or its RCW is collected.
    // Register lazily so constructing a descriptor does not touch WinUI.
    private static DependencyProperty? s_ownedHeaderProperty;
    private static DependencyProperty OwnedHeaderProperty => s_ownedHeaderProperty ??=
        DependencyProperty.RegisterAttached(
            "ReactorOwnedSettingsHeader", typeof(object), typeof(SettingsHeaderBridge), new PropertyMetadata(null));

    private static HeaderState? GetOwnedHeader(FrameworkElement control) =>
        s_ownedHeaderProperty is { } property ? control.GetValue(property) as HeaderState : null;

    internal static ControlDescriptor<TElement, TControl> WithSettingsHeader<TElement, TControl>(
        this ControlDescriptor<TElement, TControl> descriptor,
        Func<TElement, Element?> getHeader,
        Func<TElement, string?> getText,
        Func<DependencyProperty> getHeaderProperty)
        where TElement : Element
        where TControl : FrameworkElement, new() =>
        descriptor.ImperativeBridged(
            mount: (ctx, control, element) =>
            {
                var header = getHeader(element);
                var text = getText(element);
                if (header is null && text is null) return;
                var state = new HeaderState();
                control.SetValue(OwnedHeaderProperty, state);
                state.Child = header is null ? null : ctx.MountChild(header);
                state.Value = header is null ? text : state.Child;
                control.SetValue(getHeaderProperty(), state.Value);
            },
            update: (ctx, control, oldElement, newElement) =>
            {
                var oldHeader = getHeader(oldElement);
                var header = getHeader(newElement);
                if (header is not null)
                {
                    var state = GetOwnedHeader(control);
                    var hadHeader = state is not null;
                    if (state is null)
                    {
                        state = new HeaderState();
                        control.SetValue(OwnedHeaderProperty, state);
                    }
                    state.Child = ctx.ReconcileChild(hadHeader ? oldHeader : null, header, state.Child);
                    state.Value = state.Child;
                    if (!ReferenceEquals(control.GetValue(getHeaderProperty()), state.Value))
                        control.SetValue(getHeaderProperty(), state.Value);
                    return;
                }
                var text = getText(newElement);
                if (oldHeader is null && getText(oldElement) == text) return;
                ReleaseHeader(ctx.Reconciler, control, getHeaderProperty(), clearHeader: text is null);
                if (text is not null)
                {
                    control.SetValue(OwnedHeaderProperty, new HeaderState { Value = text });
                    control.SetValue(getHeaderProperty(), text);
                }
            },
            unmount: (ctx, control) =>
                ReleaseHeader(ctx.Reconciler, control, getHeaderProperty(), clearHeader: true));

    private static void ReleaseHeader(
        Reconciler reconciler, FrameworkElement control, DependencyProperty headerProperty, bool clearHeader)
    {
        if (GetOwnedHeader(control) is not { } state) return;
        control.ClearValue(OwnedHeaderProperty);
        if (state.Child is { } child) reconciler.UnmountChild(child);
        var current = control.GetValue(headerProperty);
        if (clearHeader && (ReferenceEquals(current, state.Value) ||
            current is string text && state.Value is string ownedText && text == ownedText))
            control.ClearValue(headerProperty);
    }
}