using AchievementTranslator.Models;

namespace AchievementTranslator.Services;

public class TranslationService
{
    private AchievementStringMap _stringMap = new();
    private List<Achievement> _achievements = new();

    public string CurrentLanguage { get; private set; } = "en_us";
    public string InfoMapPath { get; private set; } = string.Empty;
    public string StringMapPath { get; private set; } = string.Empty;
    public bool IsLoaded => _achievements.Count > 0;

    public void Load(string infoMapPath, string stringMapPath)
    {
        InfoMapPath = infoMapPath;
        StringMapPath = stringMapPath;
        _achievements = JsonLoader.LoadAchievements(infoMapPath);
        _stringMap = JsonLoader.LoadStringMap(stringMapPath);
    }

    public void Save() => JsonLoader.SaveStringMap(StringMapPath, _stringMap);

    public List<Achievement> GetAchievements() => _achievements;

    public List<string> GetLanguages()
    {
        var langs = _stringMap.GetAllLanguages().ToList();
        langs.Sort();
        langs.Remove("en_us");
        langs.Insert(0, "en_us");
        return langs;
    }

    public void SetLanguage(string language) => CurrentLanguage = language;

    /// <summary>
    /// Adds a new language code to the string map, pre-filling entries with empty strings
    /// so the translator can see all fields immediately.
    /// </summary>
    public void AddLanguage(string langCode)
    {
        var ids = _achievements.SelectMany(a => a.AllStringIds()).Distinct();
        foreach (var id in ids)
        {
            var key = id.ToString();
            if (!_stringMap.ContainsKey(key))
                _stringMap[key] = new Dictionary<string, string>();
            if (!_stringMap[key].ContainsKey(langCode))
                _stringMap[key][langCode] = string.Empty;
        }
    }

    public string? GetText(ulong stringId, string? language = null)
    {
        if (stringId == 0) return null;
        return _stringMap.GetText(stringId, language ?? CurrentLanguage);
    }

    public void SetText(ulong stringId, string text)
    {
        if (stringId == 0) return;
        _stringMap.SetText(stringId, CurrentLanguage, text);
    }

    /// <summary>Overall progress: distinct string IDs that have a non-empty translation.</summary>
    public (int translated, int total) GetProgress()
    {
        var ids = _achievements.SelectMany(a => a.AllStringIds()).Distinct().ToHashSet();
        int total = ids.Count;
        int done = ids.Count(id => !string.IsNullOrWhiteSpace(_stringMap.GetText(id, CurrentLanguage)));
        return (done, total);
    }

    /// <summary>
    /// Progress broken down by category (using CategoryStr english text as key).
    /// Returns list of (categoryName, translated, total).
    /// </summary>
    public List<(string Category, int Translated, int Total)> GetProgressByCategory()
    {
        var result = new Dictionary<string, (HashSet<ulong> done, HashSet<ulong> all)>();

        foreach (var a in _achievements)
        {
            var catName = GetText(a.CategoryStr, "en_us") ?? "(sans catégorie)";
            if (!result.ContainsKey(catName))
                result[catName] = (new HashSet<ulong>(), new HashSet<ulong>());

            foreach (var id in a.AllStringIds())
            {
                result[catName].all.Add(id);
                if (!string.IsNullOrWhiteSpace(_stringMap.GetText(id, CurrentLanguage)))
                    result[catName].done.Add(id);
            }
        }

        return result
            .Select(kv => (kv.Key, kv.Value.done.Count, kv.Value.all.Count))
            .OrderBy(x => x.Key)
            .ToList();
    }

    /// <summary>
    /// Returns all achievements that share the given stringId (shared translations).
    /// </summary>
    public List<Achievement> GetAchievementsUsingStringId(ulong stringId)
        => _achievements.Where(a => a.AllStringIds().Contains(stringId)).ToList();
}
