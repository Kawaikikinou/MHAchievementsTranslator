using Newtonsoft.Json;

namespace AchievementTranslator.Models;

/// <summary>
/// Safely deserializes JSON integers that may exceed long.MaxValue into ulong.
/// Newtonsoft.Json reads large integers as negative longs due to overflow;
/// this converter re-interprets the bit pattern correctly using unchecked cast.
/// </summary>
public class ULongConverter : JsonConverter<ulong>
{
    public override ulong ReadJson(JsonReader reader, Type objectType, ulong existingValue,
                                   bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return 0;

        if (reader.TokenType == JsonToken.Integer)
        {
            // Newtonsoft stores the value as long internally.
            // Values > long.MaxValue come back as negative longs due to overflow.
            // unchecked cast reinterprets the bit pattern as ulong correctly.
            if (reader.Value is long l)
                return unchecked((ulong)l);
            if (reader.Value is ulong u)
                return u;
            // Fallback: parse raw string representation
            return ulong.TryParse(reader.Value?.ToString(), out var parsed) ? parsed : 0;
        }

        if (reader.TokenType == JsonToken.String)
            return ulong.TryParse((string?)reader.Value, out var s) ? s : 0;

        return 0;
    }

    public override void WriteJson(JsonWriter writer, ulong value, JsonSerializer serializer)
        => writer.WriteValue(value);
}
