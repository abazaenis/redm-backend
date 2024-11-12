namespace Redm_backend.Dtos.User
{
	public class UpdatePasswordDto
	{
		public string OldPassword { get; set; } = string.Empty;

		public string NewPassword { get; set; } = string.Empty;

		public string ConfirmPassword { get; set; } = string.Empty;
	}
}