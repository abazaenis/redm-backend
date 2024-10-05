namespace Redm_backend.Dtos.PeriodHistory
{
	using System.Text.Json.Serialization;

	public class DateActionDto
	{
		public DateTime Date { get; set; }

		[JsonConverter(typeof(ActionConverter))]
		public ActionType Action { get; set; }
	}
}
