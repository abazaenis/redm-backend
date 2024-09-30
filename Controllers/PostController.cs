namespace Redm_backend.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Redm_backend.Dtos.Post;
    using Redm_backend.Dtos.PostCategory;
    using Redm_backend.Models;
    using Redm_backend.Services.PostService;
    using Swashbuckle.AspNetCore.Annotations;

    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost("AddPost")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Requires admin privileges")]
        public async Task<ActionResult<ServiceResponse<object?>>> AddPost(AddPostDto post)
        {
            var response = await _postService.AddPost(post);

            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Created(string.Empty, response);
        }

        [HttpGet("GetPostsPreviews")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<ServiceResponse<List<GetPostCategoryDto>>>> GetPostsPreviews()
        {
            var response = await _postService.GetPostsPreviews();

            return Ok(response);
        }

        [HttpGet("GetPost")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<ServiceResponse<GetPostDto>>> GetPost(int postId)
        {
            var response = await _postService.GetPost(postId);

            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPut("UpdatePost")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Requires admin privileges")]
        public async Task<ActionResult<ServiceResponse<object?>>> UpdatePost(UpdatePostDto post)
        {
            var response = await _postService.UpdatePost(post);

            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpDelete("DeletePost")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Requires admin privileges")]
        public async Task<ActionResult<ServiceResponse<object?>>> DeletePost(int postId)
        {
            var response = await _postService.DeletePost(postId);

            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}