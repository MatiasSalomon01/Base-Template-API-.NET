using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApi.Configuration.Converters;

public class UtcToLocalDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var local = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        local = TimeZoneInfo.ConvertTimeFromUtc(local, TimeZoneInfo.Local);
        writer.WriteStringValue(local);
    }
}