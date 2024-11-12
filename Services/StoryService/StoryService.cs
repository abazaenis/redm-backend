namespace Redm_backend.Services.StoryService
{
	using AutoMapper;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.Story;
	using Redm_backend.Models;

	public class StoryService : IStoryService
	{
		private readonly DataContext _context;

		private readonly IMapper _mapper;

		public StoryService(DataContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<ServiceResponse<object?>> AddStory(AddSingleStoryDto story)
		{
			var response = new ServiceResponse<object?>();

			var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == story.PostId);

			if (post is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Post sa id-om {story.PostId} ne postoji";
				return response;
			}

			post.Stories.Add(_mapper.Map<Story>(story));
			await _context.SaveChangesAsync();

			response.StatusCode = 201;
			response.Message = "Uspješno ste dodali novi story";
			return response;
		}

		public async Task<ServiceResponse<object?>> DeleteStory(int storyId)
		{
			var response = new ServiceResponse<object?>();

			var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == storyId);
			if (story is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Story sa id-om {storyId} ne postoji.";
				return response;
			}

			_context.Stories.Remove(story);
			await _context.SaveChangesAsync();

			response.Message = "Uspješno ste obrisali story.";
			return response;
		}
	}
}