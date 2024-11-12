namespace Redm_backend.Dtos.Post
{
	using Redm_backend.Dtos.Story;

	public class AddPostDto
	{
		public int PostCategoryId { get; set; }

		public string? Title { get; set; }

		public string? Image { get; set; }

		public List<AddStoryDto>? Stories { get; set; }
	}
}