using Click_Test.Hooks;
using Microsoft.UI.Reactor.Core;

namespace Click_Test;

public static class AppContexts
{
    public static readonly Context<PreferredThemeState> Theme = new(default);
}