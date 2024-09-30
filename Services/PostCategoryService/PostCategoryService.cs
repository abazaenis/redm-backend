namespace Redm_backend.Services.AdminService
{
    using System.Collections.Generic;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Redm_backend.Data;
    using Redm_backend.Dtos.Post;
    using Redm_backend.Dtos.PostCategory;
    using Redm_backend.Models;

    public class PostCategoryService : IPostCategoryService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PostCategoryService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<PostCategory>> AddPostCategory(CreatePostCategoryDto postCategoryDto)
        {
            var response = new ServiceResponse<PostCategory>();

            if (postCategoryDto.Title == null || postCategoryDto.Title.Length == 0)
            {
                response.Message = "Naziv kategorije mora imati barem jedan karakter";
                response.StatusCode = 400;

                return response;
            }

            var categoryExists = await _context.PostCategories.AnyAsync(pc => pc.Title == postCategoryDto.Title);

            if (categoryExists)
            {
                response.Message = $"Kategorija '{postCategoryDto.Title}' već postoji.";
                response.StatusCode = 400;
                return response;
            }

            var newPostCategory = new PostCategory { Title = postCategoryDto.Title };
            _context.PostCategories.Add(newPostCategory);
            await _context.SaveChangesAsync();

            response.StatusCode = 201;
            response.Data = newPostCategory;
            response.Message = "Uspješno ste dodali kategoriju";
            return response;
        }

        public async Task<ServiceResponse<List<PostCategoryPreviewDto>>> GetPostCategories()
        {
            var serviceResponse = new ServiceResponse<List<PostCategoryPreviewDto>>();
            var postCategories = await _context.PostCategories.OrderBy(pc => pc.Title).ToListAsync();

            serviceResponse.Data = _mapper.Map<List<PostCategoryPreviewDto>>(postCategories);
            return serviceResponse;
        }

        public async Task<ServiceResponse<object?>> UpdatePostCategory(UpdatePostCategoryDto postCategory)
        {
            var response = new ServiceResponse<object?>();
            var postCategoryDb = await _context.PostCategories.FirstOrDefaultAsync(pc => pc.Id == postCategory.Id);

            if (postCategoryDb is null)
            {
                response.StatusCode = 404;
                response.DebugMessage = $"Kategorija sa sa id-om '{postCategory.Id}' ne postoji.";
                return response;
            }

            var alreadyExists = await _context.PostCategories.AnyAsync(pc => (pc.Title == postCategory.Title) && (pc.Id != postCategory.Id));

            if (alreadyExists)
            {
                response.StatusCode = 400;
                response.Message = $"Kategorija sa nazivom '{postCategory.Title}' već postoji";
                return response;
            }

            postCategoryDb.Title = postCategory.Title;
            await _context.SaveChangesAsync();

            response.Message = "Uspješno ste ažurirali kategoriju";
            return response;
        }

        public async Task<ServiceResponse<object?>> DeletePostCategory(int postCategoryId)
        {
            var response = new ServiceResponse<object?>();
            var postCategory = await _context.PostCategories.FirstOrDefaultAsync(pc => pc.Id == postCategoryId);

            if (postCategory is null)
            {
                response.StatusCode = 404;
                response.DebugMessage = $"Ne postoji kategorija sa id-om {postCategoryId}";
                return response;
            }

            _context.PostCategories.Remove(postCategory);
            await _context.SaveChangesAsync();

            response.Message = "Uspješno ste obrisali kategoriju.";
            return response;
        }
    }
}