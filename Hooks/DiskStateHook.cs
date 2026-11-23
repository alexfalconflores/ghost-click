using System.Text.Json;
using Microsoft.UI.Reactor.Core;

namespace Click_Test.Hooks;

public static class DiskStateHook
{
    static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GhostClick");

    static string PathFor(string key) => Path.Combine(Folder, key + ".json");

    static T Load<T>(string key, T fallback)
    {
        try
        {
            var path = PathFor(key);
            return File.Exists(path)
                ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? fallback
                : fallback;
        }
        catch { return fallback; }
    }

    static void Save<T>(string key, T value)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathFor(key), JsonSerializer.Serialize(value));
        }
        catch { }
    }

    /// Borra el archivo de esa clave. Se puede llamar fuera de un componente.
    public static void Delete(string key)
    {
        try
        {
            var path = PathFor(key);
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
    }

    public static PersistedState<T> UseDiskState<T>(
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
            if (!EqualityComparer<T>.Default.Equals(value, defaultValue))
                skipNextSave.Current = true;
            setValue(defaultValue);
        }

        return new PersistedState<T>(value, v => setValue(v), Remove);
    }
}