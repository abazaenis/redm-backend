namespace Redm_backend.Services.PostService
{
	using System.Collections.Generic;

	using AutoMapper;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.PostCategory;
	using Redm_backend.Dtos.Story;
	using Redm_backend.Models;

	public class PostService : IPostService
	{
		private readonly DataContext _context;
		private readonly IMapper _mapper;

		public PostService(DataContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<ServiceResponse<object?>> AddPost(AddPostDto post)
		{
			var response = new ServiceResponse<object?>();

			var categoryExists = await _context.PostCategories.AnyAsync(pc => pc.Id == post.PostCategoryId);

			if (!categoryExists)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Kategorija sa id-em {post.PostCategoryId} ne postoji";
				return response;
			}

			var newPost = new Post { PostCategoryId = post.PostCategoryId, Title = post.Title, Image = post.Image };

			_context.Posts.Add(newPost);
			await _context.SaveChangesAsync();

			if (post.Stories != null)
			{
				foreach (AddStoryDto story in post.Stories)
				{
					_context.Stories.Add(new Story
					{
						PostId = newPost.Id,
						Title = story.Title,
						Image = story.Image,
						BackgroundColor = story.BackgroundColor,
					});
				}
			}

			await _context.SaveChangesAsync();

			response.Message = "Uspješno ste dodali post";
			return response;
		}

		public async Task<ServiceResponse<GetPostDto>> GetPost(int postId)
		{
			var response = new ServiceResponse<GetPostDto>();
			var post = await _context.Posts.Include(p => p.Stories).FirstOrDefaultAsync(pc => pc.Id == postId);

			if (post is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Ne postoji post sa id-om {postId}";
				return response;
			}

			response.Data = _mapper.Map<GetPostDto>(post);
			return response;
		}

		public async Task<ServiceResponse<object?>> UpdatePost(UpdatePostDto post)
		{
			var response = new ServiceResponse<object?>();

			var postDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == post.Id);

			if (postDb is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Post sa id-om {post.Id} ne postoji.";
				return response;
			}

			postDb.Title = post.Title;
			postDb.Image = post.Image;
			await _context.SaveChangesAsync();

			response.Message = "Uspješno ste ažurirali post";
			return response;
		}

		public async Task<ServiceResponse<object?>> DeletePost(int postId)
		{
			var response = new ServiceResponse<object?>();

			var postDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);

			if (postDb is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Post sa id-om {postId} ne postoji.";
				return response;
			}

			_context.Posts.Remove(postDb);
			await _context.SaveChangesAsync();

			response.Message = "Uspješno ste obrisali post";
			return response;
		}

		public async Task<ServiceResponse<List<GetPostCategoryDto>>> GetPostsPreviews()
		{
			var response = new ServiceResponse<List<GetPostCategoryDto>>();
			var previews = await _context.PostCategories.Where(p => p.Posts.Count != 0).Include(p => p.Posts).OrderBy(pc => pc.Title).ToListAsync();

			response.Data = _mapper.Map<List<GetPostCategoryDto>>(previews);

			return response;
		}
	}
}