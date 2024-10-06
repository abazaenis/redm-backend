namespace Redm_backend.Dtos.PeriodHistory
{
    using System.Text.Json.Serialization;

    public class GetPeriodDto
    {
        public int Id { get; set; }

        public bool Selected { get; set; } = true;

        [JsonConverter(typeof(CalendarColorConverter))]
        public CalendarColor Color { get; set; } = CalendarColor.Period;

        public string TextColor { get; set; } = "#000";

        public bool StartingDay { get; set; } = false;

        public bool EndingDay { get; set; } = false;

        public int DayIndex { get; set; }
    }
}