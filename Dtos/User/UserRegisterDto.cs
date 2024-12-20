﻿namespace Redm_backend.Dtos.User
{
	public class UserRegisterDto
	{
		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;

		public string? ExpoPushToken { get; set; } = null;
	}
}