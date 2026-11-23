using System.Text.Json.Serialization;

namespace Click_Test.Hooks;

// Un [JsonSerializable] por cada tipo que guardes con UseDiskState
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
internal partial class AppJsonContext : JsonSerializerContext;