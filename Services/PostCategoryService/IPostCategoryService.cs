namespace Redm_backend.Services.AdminService
{
	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.PostCategory;
	using Redm_backend.Models;

	public interface IPostCategoryService
	{
		Task<ServiceResponse<PostCategory>> AddPostCategory(CreatePostCategoryDto postCategoryDto);

		Task<ServiceResponse<List<PostCategoryPreviewDto>>> GetPostCategories();

		Task<ServiceResponse<object?>> UpdatePostCategory(UpdatePostCategoryDto postCategory);

		Task<ServiceResponse<object?>> DeletePostCategory(int postCategoryId);
	}
}