namespace Redm_backend.Services.UserService
{
	using System.Security.Claims;
	using System.Text.RegularExpressions;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.User;
	using Redm_backend.Models;

	public class UserService : IUserService
	{
		private readonly DataContext _context;
		private readonly IAuthRepository _authRepository;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UserService(DataContext context, IAuthRepository authRepository, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_authRepository = authRepository;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ServiceResponse<AccessToken>> UpdateUser(UserDto user)
		{
			var response = new ServiceResponse<AccessToken>();

			var tempUser = new User
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email,
			};

			_authRepository.CheckRegisterRequestValues(tempUser, out bool validRequest, out string message);

			if (!validRequest)
			{
				response.StatusCode = 400;
				response.Message = message;
				return response;
			}

			var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (userDb is null)
			{
				response.StatusCode = 404;
				response.Message = "Trenutno nismo u mogućnosti da ažuriramo korisnika.";
				response.DebugMessage = "Ne postoji korisnik sa trenutnim ID-om";
				return response;
			}

			var createdWithGoogle = !string.IsNullOrEmpty(userDb.GoogleId);
			var createdWithApple = !string.IsNullOrEmpty(userDb.AppleId);

			if ((createdWithGoogle || createdWithApple) && (userDb.Email != user.Email))
			{
				response.Message = createdWithGoogle
					? "Ne možete promijeniti email ukoliko ste izvršili prijavu preko Googlea."
					: "Ne možete promijeniti email ukoliko ste izvršili prijavu preko Applea.";
				response.DebugMessage = string.Empty;
				response.StatusCode = 400;
				return response;
			}

			var userExists = await _context.Users.AnyAsync(u => u.Email == user.Email && u.Email != userDb.Email);

			if (userExists)
			{
				response.StatusCode = 400;
				response.Message = $"Korisnik '{user.Email}' već postoji.";
				return response;
			}

			userDb.Email = user.Email;
			userDb.FirstName = user.FirstName;
			userDb.LastName = user.LastName;
			await _context.SaveChangesAsync();

			var newAccessToken = new AccessToken
			{
				NewAccessToken = _authRepository.CreateToken(userDb, userDb.PasswordHash != null),
			};

			response.Data = newAccessToken;
			response.Message = "Uspješno ste ažurirali profil.";

			return response;
		}

		public async Task<ServiceResponse<object?>> UpdatePassword(UpdatePasswordDto passwordDto)
		{
			var response = new ServiceResponse<object?>();

			if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
			{
				response.StatusCode = 400;
				response.Message = "Lozinke se ne poklapaju";
				return response;
			}

			if (passwordDto.ConfirmPassword.Length < 6)
			{
				response.StatusCode = 400;
				response.Message = "Lozinka mora imati barem 6 karaktera.";
				return response;
			}

			var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (userDb is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = "Korisnik ne postoji";
				return response;
			}

			var createdWithGoogle = !string.IsNullOrEmpty(userDb.GoogleId);
			var createdWithApple = !string.IsNullOrEmpty(userDb.AppleId);

			if (createdWithGoogle || createdWithApple)
			{
				response.StatusCode = 400;
				response.Message = createdWithGoogle
					? "Ne možete ažurirati password ukoliko ste se prijavili preko Googlea."
					: "Ne možete ažurirati password ukoliko ste se prijavili preko Applea.";
				return response;
			}

			if (!_authRepository.VerifyPasswordHash(passwordDto.OldPassword, userDb.PasswordHash!, userDb.PasswordSalt!))
			{
				response.StatusCode = 400;
				response.Message = "Pogrešna lozinka.";
				return response;
			}

			_authRepository.CreatePasswordHash(passwordDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
			userDb.PasswordHash = passwordHash;
			userDb.PasswordSalt = passwordSalt;

			await _context.SaveChangesAsync();

			response.Message = "Uspješno ste ažurirali password";

			return response;
		}

		public async Task<ServiceResponse<AccessToken>> UpdateCycleInfo(CycleInfoDto cycleInfo)
		{
			var response = new ServiceResponse<AccessToken>();

			if (cycleInfo.PeriodDuration < 2 || cycleInfo.PeriodDuration > 10)
			{
				response.StatusCode = 400;
				response.Message = "Trajanje menstruacije može biti najmanje 1 dan i najviše 10 dana.";

				return response;
			}
			else if (cycleInfo.CycleDuration < 21 || cycleInfo.CycleDuration > 45)
			{
				response.StatusCode = 400;
				response.Message = "Trajanje ciklusa može biti najmanje 21 dan i najviše 45 dana.";

				return response;
			}

			var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (userDb is null)
			{
				response.DebugMessage = "Korisnik ne postoji";
				response.StatusCode = 404;

				return response;
			}

			userDb.PeriodDuration = cycleInfo.PeriodDuration;
			userDb.CycleDuration = cycleInfo.CycleDuration;
			await _context.SaveChangesAsync();

			var newAccessToken = new AccessToken
			{
				NewAccessToken = _authRepository.CreateToken(userDb, userDb.PasswordHash != null),
			};

			response.Data = newAccessToken;
			response.DebugMessage = "Uspješno ažurirane informacije o periodu";
			return response;
		}

		public async Task<ServiceResponse<AccessToken>> UpdateAvatarName(string avatarName)
		{
			var response = new ServiceResponse<AccessToken>();

			if (avatarName.Length == 0)
			{
				response.StatusCode = 400;
				response.Message = "Ime avatara mora imati makar jedan karakter";

				return response;
			}

			var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (userDb is null)
			{
				response.DebugMessage = "Korisnik ne postoji";
				response.StatusCode = 404;

				return response;
			}

			userDb.AvatarName = avatarName;
			await _context.SaveChangesAsync();

			var newAccessToken = new AccessToken
			{
				NewAccessToken = _authRepository.CreateToken(userDb, userDb.PasswordHash != null),
			};

			response.Data = newAccessToken;
			response.DebugMessage = "Uspješno ažurirano ime avatara.";
			return response;
		}

		public async Task<ServiceResponse<object?>> UpdateExpoPushToken(ExpoPushTokenDto expoPushToken)
		{
			var response = new ServiceResponse<object?>();
			string pattern = @"^ExponentPushToken\[(.*?)\]$";

			if (string.IsNullOrWhiteSpace(expoPushToken.ExpoPushToken) || !Regex.IsMatch(expoPushToken.ExpoPushToken, pattern))
			{
				response.DebugMessage = "ExpoPushToken mora biti u formatu 'ExponentPushToken[...]'.";
				response.StatusCode = 400;

				return response;
			}

			var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (userDb is null)
			{
				response.DebugMessage = "Korisnik ne postoji";
				response.StatusCode = 404;

				return response;
			}

			userDb.ExpoPushToken = expoPushToken.ExpoPushToken;
			await _context.SaveChangesAsync();

			response.DebugMessage = "Uspješno ažuriran ExpoPushToken.";
			return response;
		}

		public async Task<ServiceResponse<object?>> DeleteUser()
		{
			var response = new ServiceResponse<object?>();
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (user is not null)
			{
				_context.Users.Remove(user);
				await _context.SaveChangesAsync();

				response.Message = "Uspješno ste obrisali vaš profil";
			}
			else
			{
				response.StatusCode = 404;
				response.DebugMessage = "Korisnik ne postoji.";
			}

			return response;
		}

		public async Task<ServiceResponse<object?>> DeleteExpoPushToken()
		{
			var response = new ServiceResponse<object?>();

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (user is not null)
			{
				user.ExpoPushToken = null;
				await _context.SaveChangesAsync();

				response.Message = "Uspješno ste ugasili notifikacije.";
			}
			else
			{
				response.StatusCode = 404;
				response.DebugMessage = "Korisnik ne postoji.";
			}

			return response;
		}

		public int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue("UserId")!);

		public async Task<bool> UserExists(string email)
		{
			if (await _context.Users.AnyAsync(u => u.Email == email))
			{
				return true;
			}

			return false;
		}

		public async Task<ServiceResponse<object?>> UpdateLastActive()
		{
			var response = new ServiceResponse<object?>();

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

			if (user is not null)
			{
				user.LastActive = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				response.Message = "Uspješno zabilježen log.";
			}
			else
			{
				response.StatusCode = 404;
				response.DebugMessage = "Korisnik ne postoji.";
			}

			return response;
		}
	}
}