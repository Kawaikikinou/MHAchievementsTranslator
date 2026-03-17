using Newtonsoft.Json;
using AchievementTranslator.Models;
using System.IO;

namespace AchievementTranslator.Services;

public static class JsonLoader
{
    private static readonly JsonSerializerSettings _settings = new()
    {
        // ULongConverter is applied via [JsonConverter] attributes on model properties.
        // We still use default settings here; the attribute-level converters take priority.
    };

    public static List<Achievement> LoadAchievements(string path)
    {
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<List<Achievement>>(json, _settings)
               ?? throw new InvalidDataException("Failed to parse AchievementInfoMap.json");
    }

    public static AchievementStringMap LoadStringMap(string path)
    {
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<AchievementStringMap>(json)
               ?? throw new InvalidDataException("Failed to parse AchievementStringMap.json");
    }

    public static void SaveStringMap(string path, AchievementStringMap map)
    {
        // Sort keys for stable diffs
        var sorted = new SortedDictionary<string, Dictionary<string, string>>(
            map, StringComparer.Ordinal);
        var json = JsonConvert.SerializeObject(sorted, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}
