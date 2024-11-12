namespace Redm_backend.Dtos.Auth
{
	public class TokensResponseDto
	{
		public string AccessToken { get; set; } = string.Empty;

		public string RefreshToken { get; set; } = string.Empty;
	}
}