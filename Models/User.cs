namespace Redm_backend.Models
{
	public class User
	{
		public int Id { get; set; }

		public string Role { get; set; } = "User";

		public string? FirstName { get; set; }

		public string? LastName { get; set; }

		public string? Email { get; set; }

		public byte[]? PasswordHash { get; set; } = new byte[0];

		public byte[]? PasswordSalt { get; set; } = new byte[0];

		public string? GoogleId { get; set; } = string.Empty;

		public string? AppleId { get; set; } = string.Empty;

		public int PeriodDuration { get; set; } = 5;

		public int CycleDuration { get; set; } = 28;

		public string? AvatarName { get; set; } = "CicaMica";

		public RefreshToken? RefreshToken { get; set; }

		public string? ExpoPushToken { get; set; }

		public DateTime? LastActive { get; set; }

		public ICollection<PeriodHistory>? PeriodHistories { get; set; }
	}
}