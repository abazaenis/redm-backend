namespace Redm_backend.Models
{
	public class Symptom
	{
		public int Id { get; set; }

		public int UserId { get; set; }

		public DateTime Date { get; set; }

		public List<string> PhysicalSymptoms { get; set; } = new List<string>();

		public List<string> MoodSymptoms { get; set; } = new List<string>();

		public List<string> SexualActivitySymptoms { get; set; } = new List<string>();

		public List<string> OtherSymptoms { get; set; } = new List<string>();

		// Navigation property
		public User? User { get; set; }
	}
}