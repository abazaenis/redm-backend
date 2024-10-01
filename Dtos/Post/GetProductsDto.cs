namespace Redm_backend.Dtos.Post
{
	using Redm_backend.Dtos.Product;

	public class GetProductsDto
	{
		public int Id { get; set; }

		public string? Title { get; set; }

		public List<GetProductDto> Products { get; set; } = new List<GetProductDto>();
	}
}
