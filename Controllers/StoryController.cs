namespace Redm_backend.Controllers
{
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Dtos.Story;
	using Redm_backend.Models;
	using Redm_backend.Services.StoryService;

	using Swashbuckle.AspNetCore.Annotations;

	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin")]
	public class StoryController : ControllerBase
	{
		private readonly IStoryService _storyService;

		public StoryController(IStoryService storyService)
		{
			_storyService = storyService;
		}

		[HttpPost("AddStory")]
		[SwaggerOperation(Summary = "Requires admin privileges")]
		public async Task<ActionResult<ServiceResponse<object?>>> AddStory(AddSingleStoryDto story)
		{
			var response = await _storyService.AddStory(story);

			if (response.StatusCode == 404)
			{
				return NotFound(response);
			}

			return Created(string.Empty, response);
		}

		[HttpDelete("DeleteStory")]
		[SwaggerOperation(Summary = "Requires admin privileges")]
		public async Task<ActionResult<ServiceResponse<object?>>> DeleteStory(int storyId)
		{
			var response = await _storyService.DeleteStory(storyId);

			if (response.StatusCode == 404)
			{
				return NotFound(response);
			}

			return Ok(response);
		}
	}
}