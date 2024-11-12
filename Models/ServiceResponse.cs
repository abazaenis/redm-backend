namespace Redm_backend.Models
{
	public class ServiceResponse<T>
	{
		public T? Data { get; set; }

		public string DebugMessage { get; set; } = string.Empty;

		public string Message { get; set; } = string.Empty;

		public int StatusCode { get; set; } = 200;
	}
}