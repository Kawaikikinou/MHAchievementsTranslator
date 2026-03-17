using Newtonsoft.Json;

namespace AchievementTranslator.Models;

public class Achievement
{
    [JsonProperty("Id")]
    public long Id { get; set; }

    [JsonProperty("Enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("ParentId")]
    public long ParentId { get; set; }

    [JsonProperty("Name")]
    [JsonConverter(typeof(ULongConverter))]
    public ulong Name { get; set; }

    [JsonProperty("InProgressStr")]
    [JsonConverter(typeof(ULongConverter))]
    public ulong InProgressStr { get; set; }

    [JsonProperty("CompletedStr")]
    [JsonConverter(typeof(ULongConverter))]
    public ulong CompletedStr { get; set; }

    [JsonProperty("RewardStr")]
    [JsonConverter(typeof(ULongConverter))]
    public ulong RewardStr { get; set; }

    [JsonProperty("CategoryStr")]
    [JsonConverter(typeof(ULongConverter))]
    public ulong CategoryStr { get; set; }

    [JsonProperty("SubCategoryStr")]
    [JsonConverter(typeof(ULongConverter))]
    public ulong SubCategoryStr { get; set; }

    [JsonProperty("DisplayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>Returns all non-zero translatable string IDs for this achievement.</summary>
    public IEnumerable<ulong> AllStringIds()
    {
        if (Name != 0)           yield return Name;
        if (InProgressStr != 0)  yield return InProgressStr;
        if (CompletedStr != 0)   yield return CompletedStr;
        if (RewardStr != 0)      yield return RewardStr;
        if (CategoryStr != 0)    yield return CategoryStr;
        if (SubCategoryStr != 0) yield return SubCategoryStr;
    }
}
