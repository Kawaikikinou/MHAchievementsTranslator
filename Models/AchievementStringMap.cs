namespace AchievementTranslator.Models;

/// <summary>
/// Represents the entire string map: stringId -> (languageCode -> text)
/// Keys are stored as strings (decimal representation of ulong) to match JSON.
/// </summary>
public class AchievementStringMap : Dictionary<string, Dictionary<string, string>>
{
    public string? GetText(ulong stringId, string language)
    {
        var key = stringId.ToString();
        if (TryGetValue(key, out var langs) && langs.TryGetValue(language, out var text))
            return text;
        return null;
    }

    public void SetText(ulong stringId, string language, string text)
    {
        var key = stringId.ToString();
        if (!TryGetValue(key, out var langs))
        {
            langs = new Dictionary<string, string>();
            this[key] = langs;
        }
        langs[language] = text;
    }

    public HashSet<string> GetAllLanguages()
    {
        var langs = new HashSet<string>();
        foreach (var entry in Values)
            foreach (var lang in entry.Keys)
                langs.Add(lang);
        return langs;
    }
}
