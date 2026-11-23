using Microsoft.UI.Reactor.Core;
using Windows.Storage;

namespace Click_Test.Hooks;

public static class LocalSettingHook
{
    static T Load<T>(string key, T fallback)
    {
        try
        {
            var values = ApplicationData.Current.LocalSettings.Values;
            return values.TryGetValue(key, out var v) && v is T t ? t : fallback;
        }
        catch { return fallback; }
    }

    static void Save<T>(string key, T value)
    {
        try
        {
            var values = ApplicationData.Current.LocalSettings.Values;
            if (value is null) values.Remove(key);   // LocalSettings no acepta null
            else values[key] = value;
        }
        catch { }
    }

    /// Borra la clave de LocalSettings. Se puede llamar fuera de un componente.
    public static void Delete(string key)
    {
        try { ApplicationData.Current.LocalSettings.Values.Remove(key); }
        catch { }
    }

    public static PersistedState<T> UseLocalSetting<T>(
        this RenderContext ctx, string key, T defaultValue, bool persist = true)
    {
        var initial = ctx.UseMemo(
            () => persist ? Load(key, defaultValue) : defaultValue,
            []);
        var (value, setValue) = ctx.UseState(initial);
        var skipNextSave = ctx.UseRef(false);

        ctx.UseEffect(() =>
        {
            if (skipNextSave.Current) { skipNextSave.Current = false; return () => { }; }
            if (persist) Save(key, value);
            return () => { };
        }, value, persist);

        void Remove()
        {
            Delete(key);
            // Volver al valor por defecto sin que el efecto lo guarde otra vez.
            if (!EqualityComparer<T>.Default.Equals(value, defaultValue))
                skipNextSave.Current = true;
            setValue(defaultValue);
        }

        return new PersistedState<T>(value, v => setValue(v), Remove);
    }
}