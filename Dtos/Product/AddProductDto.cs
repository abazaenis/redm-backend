namespace Redm_backend.Dtos.Product
{
	public class AddProductDto
	{
		public string? Title { get; set; }

		public string? Image { get; set; }

		public string? ArticleUrl { get; set; }

		public double Price { get; set; }

		public int CategoryId { get; set; }
	}
}
