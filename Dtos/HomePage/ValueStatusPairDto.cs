namespace Redm_backend.Dtos.HomePage
{
	using System.Text.Json.Serialization;

	public class ValueStatusPairDto<T>
	{
		public T? Value { get; set; }

		[JsonConverter(typeof(StatusConverter))]
		public Status Status { get; set; }
	}
}
