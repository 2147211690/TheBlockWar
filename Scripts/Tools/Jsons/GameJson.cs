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
    extension(object obj)
    {
        public string ToJson()
        {
            return JsonSerializer.Serialize(obj, JsonOptions);
        }
    }
    public static T? FromJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}