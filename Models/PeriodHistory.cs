namespace Redm_backend.Models
{
	public class PeriodHistory
	{
		public int Id { get; set; }

		public int UserId { get; set; }

		public User? User { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }
	}
}