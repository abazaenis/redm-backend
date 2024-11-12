namespace Redm_backend.Services.PostService
{
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.PostCategory;
	using Redm_backend.Models;

	public interface IPostService
	{
		Task<ServiceResponse<object?>> AddPost(AddPostDto post);

		Task<ServiceResponse<List<GetPostCategoryDto>>> GetPostsPreviews();

		Task<ServiceResponse<GetPostDto>> GetPost(int postId);

		Task<ServiceResponse<object?>> UpdatePost(UpdatePostDto post);

		Task<ServiceResponse<object?>> DeletePost(int postId);
	}
}