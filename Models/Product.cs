namespace Redm_backend.Models
{
	public class Product
	{
		public int Id { get; set; }

		public string? Title { get; set; }

		public string? Image { get; set; }

		public string? ArticleUrl { get; set; }

		public int Price { get; set; }

		public int CategoryId { get; set; }

		public PostCategory? PostCategory { get; set; }
	}
}
