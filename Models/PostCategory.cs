namespace Redm_backend.Models
{
	public class PostCategory
	{
		public int Id { get; set; }

		public string? Title { get; set; }

		public ICollection<Post> Posts { get; set; } = new List<Post>();

		public ICollection<Product> Products { get; set; } = new List<Product>();
	}
}