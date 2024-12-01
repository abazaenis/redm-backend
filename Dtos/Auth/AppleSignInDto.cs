namespace Redm_backend.Dtos.Auth
{
	using System.ComponentModel.DataAnnotations;

	public class AppleSignInDto
	{
		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		public string Email { get; set; }

		[Required]
		public string AppleId { get; set; }
	}
}
