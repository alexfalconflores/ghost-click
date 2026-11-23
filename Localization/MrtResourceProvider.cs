using System.Collections.Concurrent;
using Microsoft.UI.Reactor.Localization;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Click_Test;

// Lee Strings/<idioma>/<Ns>.resw desde resources.pri
public sealed class MrtResourceProvider : IStringResourceProvider
{
    readonly ResourceManager _manager = new();
    readonly ConcurrentDictionary<string, ResourceContext> _contexts = new();

    public string? GetString(string locale, string ns, string key)
    {
        var context = _contexts.GetOrAdd(locale, l =>
        {
            var ctx = _manager.CreateResourceContext();
            ctx.QualifierValues["Language"] = l;
            return ctx;
        });

        // Main.resw → "Main/Clicks"
        return _manager.MainResourceMap.TryGetValue($"{ns}/{key}", context)?.ValueAsString;
    }
}