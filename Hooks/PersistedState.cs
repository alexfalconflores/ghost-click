namespace Click_Test.Hooks;

/// Lo que devuelven UseLocalSetting y UseDiskState.
/// Se puede deconstruir en 2 (value, set) o en 3 (value, set, remove).
public readonly record struct PersistedState<T>(T Value, Action<T> Set, Action Remove)
{
    public void Deconstruct(out T value, out Action<T> set)
    {
        value = Value;
        set = Set;
    }
}