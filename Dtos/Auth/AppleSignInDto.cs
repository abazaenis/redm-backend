namespace Redm_backend.Dtos.Auth
{
	using System.ComponentModel.DataAnnotations;

	public class AppleSignInDto
	{
		public string? FirstName { get; set; }

		public string? LastName { get; set; }

		public string? Email { get; set; }

		public string AppleId { get; set; }
	}
}
