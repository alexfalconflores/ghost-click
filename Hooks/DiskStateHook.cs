using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.UI.Reactor.Core;
using Windows.Storage;

namespace Click_Test.Hooks;

public static class DiskStateHook
{
    // Carpeta privada del paquete MSIX (explícita, sin depender de la virtualización)
    static readonly string Folder = ApplicationData.Current.LocalFolder.Path;

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
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiskState] Load '{key}': {ex.Message}");
            return fallback;
        }
    }

    static void Save<T>(string key, T value, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            var path = PathFor(key);
            var tmp = path + ".tmp";

            // Escritura atómica: si la app se cierra a mitad, el archivo anterior sigue intacto
            File.WriteAllText(tmp, JsonSerializer.Serialize(value, typeInfo));
            File.Move(tmp, path, overwrite: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiskState] Save '{key}': {ex.Message}");
        }
    }

    /// Borra el archivo de esa clave. Se puede llamar fuera de un componente.
    public static void Delete(string key)
    {
        try
        {
            var path = PathFor(key);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DiskState] Delete '{key}': {ex.Message}");
        }
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