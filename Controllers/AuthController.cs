namespace Redm_backend.Controllers
{
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Data;
	using Redm_backend.Dtos.Auth;
	using Redm_backend.Dtos.User;
	using Redm_backend.Models;

	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthRepository _authRepo;

		public AuthController(IAuthRepository authRepo)
		{
			_authRepo = authRepo;
		}

		[HttpPost("Register")]
		public async Task<ActionResult<ServiceResponse<TokensResponseDto>>> Register(UserRegisterDto request)
		{
			var response = await _authRepo.Register(
				new User
				{
					Email = request.Email,
					FirstName = request.FirstName,
					LastName = request.LastName,
					ExpoPushToken = request.ExpoPushToken,
				},
				request.Password);

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			return Created(string.Empty, response);
		}

		[HttpPost("Login")]
		public async Task<ActionResult<ServiceResponse<TokensResponseDto>>> Login(UserLoginDto request)
		{
			var response = await _authRepo.Login(request.Email, request.Password);

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}
			else if (response.StatusCode == 404)
			{
				return NotFound(response);
			}
			else if (response.StatusCode == 409)
			{
				return Conflict(response);
			}

			return Ok(response);
		}

		[HttpPost("Refresh")]
		public async Task<ActionResult<ServiceResponse<TokensResponseDto>>> Refresh(RefreshRequestDto request)
		{
			var response = await _authRepo.Refresh(request.RefreshToken);

			if (response.StatusCode == 401)
			{
				return Unauthorized(response);
			}

			return Ok(response);
		}

		[HttpPost("GoogleSignIn")]
		public async Task<ActionResult<ServiceResponse<TokensResponseDto>>> GoogleSignIn(GoogleSignInVMDto model)
		{
			var response = await _authRepo.GoogleSignIn(model);

			if (response.StatusCode == 200)
			{
				return Ok(response);
			}
			else if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}
			else if (response.StatusCode == 401)
			{
				return Unauthorized(response);
			}
			else if (response.StatusCode == 500)
			{
				return StatusCode(500, response);
			}
			else
			{
				return StatusCode(response.StatusCode, response);
			}
		}

		[HttpPost("AppleSignIn")]
		public async Task<ActionResult<ServiceResponse<TokensResponseDto>>> AppleSignIn(AppleSignInDto model)
		{
			var response = await _authRepo.AppleSignIn(model);

			if (response.StatusCode == 200)
			{
				return Ok(response);
			}
			else if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}
			else if (response.StatusCode == 401)
			{
				return Unauthorized(response);
			}
			else if (response.StatusCode == 500)
			{
				return StatusCode(500, response);
			}
			else
			{
				return StatusCode(response.StatusCode, response);
			}
		}
	}
}