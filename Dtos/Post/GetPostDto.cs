namespace Redm_backend.Dtos.Post
{
	using Redm_backend.Dtos.Story;

	public class GetPostDto
	{
		public int Id { get; set; }

		public int PostCategoryId { get; set; }

		public string? Title { get; set; }

		public string? Image { get; set; }

		public List<GetStoryDto> Stories { get; set; } = new List<GetStoryDto>();
	}
}