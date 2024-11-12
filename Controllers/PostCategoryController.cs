namespace Redm_backend.Controllers
{
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.PostCategory;
	using Redm_backend.Models;
	using Redm_backend.Services.AdminService;

	using Swashbuckle.AspNetCore.Annotations;

	[ApiController]
	[Route("api/[controller]")]
	public class PostCategoryController : ControllerBase
	{
		private readonly IPostCategoryService _postCategoryService;

		public PostCategoryController(IPostCategoryService postCategoryService)
		{
			_postCategoryService = postCategoryService;
		}

		[HttpPost("AddPostCategory")]
		[Authorize(Roles = "Admin")]
		[SwaggerOperation(Summary = "Requires admin privileges")]
		public async Task<ActionResult<ServiceResponse<PostCategory>>> AddPostCategory(CreatePostCategoryDto postCategoryDto)
		{
			var response = await _postCategoryService.AddPostCategory(postCategoryDto);

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			return Created(string.Empty, response);
		}

		[HttpGet("GetPostCategories")]
		[Authorize(Roles = "User,Admin")]
		public async Task<ActionResult<ServiceResponse<List<PostCategoryPreviewDto>>>> GetPostCategories()
		{
			var response = await _postCategoryService.GetPostCategories();
			return Ok(response);
		}

		[HttpPut("UpdatePostCategory")]
		[Authorize(Roles = "Admin")]
		[SwaggerOperation(Summary = "Requires admin privileges")]
		public async Task<ActionResult<ServiceResponse<object?>>> UpdatePostCategory(UpdatePostCategoryDto postCategory)
		{
			var response = await _postCategoryService.UpdatePostCategory(postCategory);

			if (response.StatusCode == 404)
			{
				return NotFound(response);
			}
			else if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			return Ok(response);
		}

		[HttpDelete("DeletePostCategory")]
		[Authorize(Roles = "Admin")]
		[SwaggerOperation(Summary = "Requires admin privileges")]
		public async Task<ActionResult<ServiceResponse<object?>>> DeletePostCategory(int postCategoryId)
		{
			var response = await _postCategoryService.DeletePostCategory(postCategoryId);

			if (response.StatusCode == 404)
			{
				return NotFound(response);
			}

			return Ok(response);
		}
	}
}