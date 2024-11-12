namespace Redm_backend.Dtos.PostCategory
{
	using Redm_backend.Dtos.Post;

	public class GetPostCategoryDto
	{
		public int Id { get; set; }

		public string? Title { get; set; }

		public List<GetPostPreviewDto> Posts { get; set; } = new List<GetPostPreviewDto>();
	}
}