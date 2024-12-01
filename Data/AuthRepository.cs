namespace Redm_backend.Data
{
	using System.IdentityModel.Tokens.Jwt;
	using System.Security.Claims;
	using System.Text.RegularExpressions;

	using Google.Apis.Auth;

	using Microsoft.EntityFrameworkCore;
	using Microsoft.IdentityModel.Tokens;

	using Redm_backend.Dtos.Auth;
	using Redm_backend.Models;

	public class AuthRepository : IAuthRepository
	{
		private readonly DataContext _context;
		private readonly IConfiguration _configuration;

		public AuthRepository(DataContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<ServiceResponse<TokensResponseDto>> Login(string email, string password)
		{
			var response = new ServiceResponse<TokensResponseDto>();
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email!.Equals(email));

			if (user is not null && user.PasswordHash is null)
			{
				response.StatusCode = 409;
				response.Message = "Registrovani ste preko Googela (moožda kasnije i Apple IDa). Molimo Vas prijavite se preko Googlea";
				return response;
			}

			// Check users username and password
			if (user is null)
			{
				response.StatusCode = 404;
				response.Message = $"Korisnik '{email}' nije pronađen.";
			}
			else if (!VerifyPasswordHash(password, user.PasswordHash!, user.PasswordSalt!))
			{
				response.StatusCode = 400;
				response.Message = "Pogrešna lozinka.";
			}
			else
			{
				// Create new access and refresh tokens
				var accessToken = CreateToken(user, user.PasswordHash != null);
				var refreshToken = Guid.NewGuid().ToString();

				// Update users token based on if user already has refresh token or not
				var existingToken = await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.UserId == user.Id);
				if (existingToken is null)
				{
					var newRefreshToken = new RefreshToken
					{
						Token = refreshToken,
						Expiration = DateTime.UtcNow.AddDays(60),
						UserId = user.Id,
					};

					_context.RefreshTokens.Add(newRefreshToken);
				}
				else
				{
					existingToken.Token = refreshToken;
					existingToken.Expiration = DateTime.UtcNow.AddDays(60);
				}

				response.Data = new TokensResponseDto()
				{
					AccessToken = accessToken,
					RefreshToken = refreshToken,
				};

				response.Message = $"Dobro došli {user.FirstName}.";
				response.DebugMessage = "Login uspješan, generisani novi access i refresh tokeni.";
			}

			await _context.SaveChangesAsync();
			return response;
		}

		public async Task<ServiceResponse<TokensResponseDto>> Register(User user, string password)
		{
			var response = new ServiceResponse<TokensResponseDto>();

			CheckRegisterRequestValues(user, out bool validRequest, out string message);

			if (!validRequest)
			{
				response.Message = message;
				response.StatusCode = 400;
				return response;
			}

			var userExists = await _context.Users.AnyAsync(u => u.Email == user.Email);

			// Check if user already exists
			if (userExists)
			{
				response.StatusCode = 400;
				response.Message = $"Korisnik {user.Email} već postoji.";
				return response;
			}

			if (password.Length < 6)
			{
				response.Message = "Lozinka mora imati barem 6 karaktera.";
				response.StatusCode = 400;
				return response;
			}

			// Register user
			CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
			user.PasswordHash = passwordHash;
			user.PasswordSalt = passwordSalt;

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			var loginResponse = await Login(user.Email!, password);

			if (loginResponse.StatusCode != 200)
			{
				response.DebugMessage = "Korisnik uspješno registrovan međutim login je neuspješan";
				response.StatusCode = 400;
				return response;
			}

			response.StatusCode = 201;
			response.Data = loginResponse.Data;
			response.Message = "Profil uspješno registrovan, molimo Vas prijavite se.";

			return response;
		}

		public async Task<ServiceResponse<TokensResponseDto>> Refresh(string refreshToken)
		{
			var response = new ServiceResponse<TokensResponseDto>();
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var validRefreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken);

				if (validRefreshToken == null)
				{
					response.StatusCode = 401;
					response.DebugMessage = "Refresh token koji ste poslali nije validan.";
					return response;
				}

				if (validRefreshToken.Expiration < DateTime.UtcNow)
				{
					response.StatusCode = 401;
					response.Message = "Sesija istekla, molimo Vas prijavite se ponovo.";
					return response;
				}

				// Generate new access and refresh tokens
				var user = await _context.Users.SingleOrDefaultAsync(user => user.Id == validRefreshToken.UserId);
				var accessToken = CreateToken(user!, user!.PasswordHash != null);
				var newRefreshToken = Guid.NewGuid().ToString();

				validRefreshToken.Token = newRefreshToken;
				validRefreshToken.Expiration = DateTime.UtcNow.AddDays(60);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				response.Data = new TokensResponseDto
				{
					AccessToken = accessToken,
					RefreshToken = newRefreshToken,
				};
				response.DebugMessage = "Zahtjev uspješan, generisani su novi access i refresh tokeni.";
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				response.StatusCode = 500;
				response.Message = "Trenutno nismo u mogućnosti da ispunimo zahtjev, pokušajte kasnije.";
				response.DebugMessage = ex.Message;
			}

			return response;
		}

		public async Task<ServiceResponse<TokensResponseDto>> GoogleSignIn(GoogleSignInVMDto model)
		{
			var response = new ServiceResponse<TokensResponseDto>();
			try
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

				if (user is null)
				{
					var userToBeCreated = new User
					{
						FirstName = model.GivenName,
						LastName = model.FamilyName,
						Email = model.Email,
						PasswordHash = null,
						PasswordSalt = null,
						GoogleId = model.Id,
					};

					_context.Users.Add(userToBeCreated);
					await _context.SaveChangesAsync();

					// Retrieve the newly created user
					user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
					response.StatusCode = 201;
				}
				else
				{
					user.GoogleId = model.Id;
					await _context.SaveChangesAsync();
				}

				var newRefreshTokenGuid = await GenerateRefreshTokenAsync(user!);

				var accessToken = CreateToken(user!, user!.PasswordHash != null);
				response.Data = new TokensResponseDto
				{
					AccessToken = accessToken,
					RefreshToken = newRefreshTokenGuid,
				};
				response.Message = $"Dobro došli {user.FirstName}.";
				response.DebugMessage = "Autentikacija uspješna, generisani novi access i refresh tokeni";
			}
			catch (InvalidJwtException ex)
			{
				response.StatusCode = 401;
				response.Message = "Trenutno nismo u mogućnosti da izvršimo prijavu preko Google-a, pokušajte kasnije.";
				response.DebugMessage = ex.Message;
			}
			catch (Exception ex)
			{
				response.StatusCode = 500;
				response.Message = "Trenutno nismo u mogućnosti da izvršimo prijavu preko Google-a, pokušajte kasnije.";
				response.DebugMessage = ex.Message;
			}

			return response;
		}

		public async Task<ServiceResponse<TokensResponseDto>> AppleSignIn(AppleSignInDto model)
		{
			var response = new ServiceResponse<TokensResponseDto>();
			try
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.AppleId == model.AppleId);

				if (user is null)
				{
					var userToBeCreated = new User
					{
						FirstName = model.FirstName,
						LastName = model.LastName,
						Email = model.Email,
						PasswordHash = null,
						PasswordSalt = null,
						AppleId = model.AppleId,
					};

					_context.Users.Add(userToBeCreated);
					await _context.SaveChangesAsync();

					// Retrieve the newly created user
					user = await _context.Users.FirstOrDefaultAsync(u => u.AppleId == model.AppleId);
					response.StatusCode = 201;
				}

				var newRefreshTokenGuid = await GenerateRefreshTokenAsync(user!);

				var accessToken = CreateToken(user!, user!.PasswordHash != null);
				response.Data = new TokensResponseDto
				{
					AccessToken = accessToken,
					RefreshToken = newRefreshTokenGuid,
				};
				response.Message = $"Dobro došli {user.FirstName}.";
				response.DebugMessage = "Autentikacija uspješna, generisani novi access i refresh tokeni";
			}
			catch (InvalidJwtException ex)
			{
				response.StatusCode = 401;
				response.Message = "Trenutno nismo u mogućnosti da izvršimo prijavu preko Applea-a, pokušajte kasnije.";
				response.DebugMessage = ex.Message;
			}
			catch (Exception ex)
			{
				response.StatusCode = 500;
				response.Message = "Trenutno nismo u mogućnosti da izvršimo prijavu preko Applea-a, pokušajte kasnije.";
				response.DebugMessage = ex.Message;
			}

			return response;
		}

		// Helper methods
		public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		{
			using var hmac = new System.Security.Cryptography.HMACSHA512();
			passwordSalt = hmac.Key;
			passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
		}

		public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
		{
			using var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt);
			var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

			return computedHash.SequenceEqual(passwordHash);
		}

		public string CreateToken(User user, bool hasProfile = true)
		{
			var claims = new List<Claim>
			{
				new Claim("UserId", user.Id.ToString()),
				new Claim("Email", user.Email ?? string.Empty),
				new Claim("FirstName", user.FirstName ?? string.Empty),
				new Claim("LastName", user.LastName ?? string.Empty),
				new Claim("PeriodDuration", user.PeriodDuration.ToString() ?? string.Empty),
				new Claim("CycleDuration", user.CycleDuration.ToString() ?? string.Empty),
				new Claim("AvatarName", user.AvatarName ?? string.Empty),
				new Claim("HasProfile", hasProfile.ToString()),
				new Claim(ClaimTypes.Role, user.Role),
			};

			var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;

			SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken!));

			SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddHours(4),
				SigningCredentials = creds,
			};

			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

		public void CheckRegisterRequestValues(User user, out bool validRequest, out string message)
		{
			if (user.FirstName is null || user.FirstName.Length == 0 || user.FirstName == string.Empty)
			{
				validRequest = false;
				message = "Ime je obavezno";
			}
			else if (user.LastName is null || user.LastName.Length == 0 || user.LastName == string.Empty)
			{
				validRequest = false;
				message = "Prezime je obavezno";
			}
			else if (user.Email is null || user.Email.Length == 0 || user.Email == string.Empty || !Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
			{
				validRequest = false;
				message = "Email nije u validnom formatu";
			}
			else
			{
				validRequest = true;
				message = string.Empty;
			}
		}

		private async Task<string> GenerateRefreshTokenAsync(User user)
		{
			var newRefreshTokenGuid = Guid.NewGuid().ToString();
			var newExpirationDate = DateTime.UtcNow.AddDays(60);
			var existingRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user!.Id);

			if (existingRefreshToken is null)
			{
				var newRefreshToken = new RefreshToken
				{
					Token = newRefreshTokenGuid,
					Expiration = newExpirationDate,
					UserId = user!.Id,
				};

				_context.RefreshTokens.Add(newRefreshToken);
			}
			else
			{
				existingRefreshToken.Token = newRefreshTokenGuid;
				existingRefreshToken.Expiration = newExpirationDate;
			}

			await _context.SaveChangesAsync();

			return newRefreshTokenGuid;
		}
	}
}