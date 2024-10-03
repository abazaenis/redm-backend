namespace Redm_backend.Dtos.Product
{
	public class GetProductDto
	{
		public int Id { get; set; }

		public string? Title { get; set; }

		public string? Image { get; set; }

		public string? ArticleUrl { get; set; }

		public double Price { get; set; }
	}
}
