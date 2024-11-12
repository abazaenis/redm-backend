namespace Redm_backend.Services.StoryService
{
	using System.Threading.Tasks;

	using Redm_backend.Dtos.Story;
	using Redm_backend.Models;

	public interface IStoryService
	{
		Task<ServiceResponse<object?>> AddStory(AddSingleStoryDto story);

		Task<ServiceResponse<object?>> DeleteStory(int storyId);
	}
}