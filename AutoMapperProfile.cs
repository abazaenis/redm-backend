namespace Redm_backend
{
	using AutoMapper;

	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.PostCategory;
	using Redm_backend.Dtos.Product;
	using Redm_backend.Dtos.Story;
	using Redm_backend.Dtos.User;
	using Redm_backend.Models;

	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<UserDto, User>();
			CreateMap<User, UserDto>();
			CreateMap<Post, GetPostDto>();
			CreateMap<Story, GetStoryDto>();
			CreateMap<AddSingleStoryDto, Story>();
			CreateMap<Post, GetPostPreviewDto>();
			CreateMap<PostCategory, PostCategoryPreviewDto>();
			CreateMap<AddProductDto, Product>();
			CreateMap<Product, GetProductDto>();
			CreateMap<PostCategory, GetProductsDto>();
			CreateMap<PostCategory, GetPostCategoryDto>();
		}
	}
}