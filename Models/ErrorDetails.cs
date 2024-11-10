namespace Redm_backend.Models
{
	using System.Text.Json;

	public class ErrorDetails
	{
		public int StatusCode { get; set; }

		public string? Message { get; set; }

		public override string ToString() => JsonSerializer.Serialize(this);
	}
}
