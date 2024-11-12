namespace Redm_backend.Models
{
	public class Story
	{
		public int Id { get; set; }

		public int PostId { get; set; }

		public Post? Post { get; set; }

		public string? Title { get; set; }

		public string? Image { get; set; }

		public string? BackgroundColor { get; set; }
	}
}