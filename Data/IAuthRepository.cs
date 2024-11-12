namespace Redm_backend.Data
{
	using Redm_backend.Dtos.Auth;
	using Redm_backend.Models;

	public interface IAuthRepository
	{
		Task<ServiceResponse<TokensResponseDto>> Register(User user, string password);

		Task<ServiceResponse<TokensResponseDto>> Login(string email, string password);

		Task<ServiceResponse<TokensResponseDto>> Refresh(string refreshToken);

		Task<ServiceResponse<TokensResponseDto>> GoogleSignIn(GoogleSignInVMDto model);

		void CheckRegisterRequestValues(User user, out bool validRequest, out string message);

		string CreateToken(User user, bool hasProfile = true);

		void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

		bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
	}
}