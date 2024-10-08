namespace Redm_backend.Dtos.PeriodHistory
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class CalendarColorConverter : JsonConverter<CalendarColor>
    {
        public override CalendarColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(); // Implement if you need deserialization
        }

        public override void Write(Utf8JsonWriter writer, CalendarColor value, JsonSerializerOptions options)
        {
            var colorString = value switch
            {
                CalendarColor.Period => "#F1B1D8",
                CalendarColor.Fertile => "#D0F2E2",
                CalendarColor.Ovulation => "#FFEB4D",
                CalendarColor.Prediction => "#FFF0F4",
                _ => "#FFFFFF" // Default color
            };

            writer.WriteStringValue(colorString);
        }
    }
}