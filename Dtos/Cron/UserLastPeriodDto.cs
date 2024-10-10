namespace Redm_backend.Dtos.Cron
{
	using Redm_backend.Models;

	public class UserLastPeriodDto
	{
		public User User { get; set; }

		public PeriodHistory? LastPeriod { get; set; }
	}
}
