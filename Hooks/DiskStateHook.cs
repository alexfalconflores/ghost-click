using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.UI.Reactor.Core;

namespace Click_Test.Hooks;

public static class DiskStateHook
{
    static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GhostClick");

    static string PathFor(string key) => Path.Combine(Folder, key + ".json");

    static T Load<T>(string key, T fallback, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            var path = PathFor(key);
            return File.Exists(path)
                ? JsonSerializer.Deserialize(File.ReadAllText(path), typeInfo) ?? fallback
                : fallback;
        }
        catch { return fallback; }
    }

    static void Save<T>(string key, T value, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathFor(key), JsonSerializer.Serialize(value, typeInfo));
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
        this RenderContext ctx, string key, T defaultValue, JsonTypeInfo<T> typeInfo, bool persist = true)
    {
        var initial = ctx.UseMemo(
            () => persist ? Load(key, defaultValue, typeInfo) : defaultValue,
            []);

        var (value, setValue) = ctx.UseState(initial);
        var skipNextSave = ctx.UseRef(false);

        ctx.UseEffect(() =>
        {
            if (skipNextSave.Current) { skipNextSave.Current = false; return () => { }; }
            if (persist) Save(key, value, typeInfo);
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