using System.Reflection;
using System.Text.Json;

namespace TheBlockWar.Scripts.Tools.Jsons;

public static class GameJson
{
    public static JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
    };

    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public static T? FromJson<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}