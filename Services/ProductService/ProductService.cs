namespace Redm_backend.Services.ProductService
{
	using AutoMapper;

	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.Hosting;

	using Redm_backend.Data;
	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.PostCategory;
	using Redm_backend.Dtos.Product;
	using Redm_backend.Models;

	public class ProductService : IProductService
	{
		private readonly DataContext _context;

		private readonly IMapper _mapper;

		public ProductService(DataContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<ServiceResponse<object?>> AddProduct(AddProductDto product)
		{
			var response = new ServiceResponse<object?>();

			CheckRequestValues(product, out string message, out bool validRequest);

			if (!validRequest)
			{
				response.Message = message;
				response.StatusCode = 400;
				return response;
			}

			var category = await _context.PostCategories.FirstOrDefaultAsync(pc => pc.Id == product.CategoryId);

			if (category == null)
			{
				response.DebugMessage = $"Kategorija sa id-om {product.CategoryId} ne postoji.";
				response.StatusCode = 404;
				return response;
			}

			category.Products.Add(_mapper.Map<Product>(product));
			await _context.SaveChangesAsync();

			response.StatusCode = 201;
			response.Message = "Uspješno ste dodali proizvod";
			return response;
		}

		public async Task<ServiceResponse<List<GetProductsDto>>> GetProductsGroupedByCategories()
		{
			var response = new ServiceResponse<List<GetProductsDto>>();
			var data = await _context.PostCategories.Where(p => p.Products.Count != 0).Include(p => p.Products).OrderBy(pc => pc.Title).ToListAsync();

			response.Data = _mapper.Map<List<GetProductsDto>>(data);

			return response;
		}

		public async Task<ServiceResponse<object?>> DeleteProduct(int productId)
		{
			var response = new ServiceResponse<object?>();

			var productDb = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

			if (productDb is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Proizvod sa id-om {productId} ne postoji.";
				return response;
			}

			_context.Products.Remove(productDb);
			await _context.SaveChangesAsync();

			response.Message = "Uspješno ste obrisali proizvod.";
			return response;
		}

		private static void CheckRequestValues(AddProductDto product, out string message, out bool validRequest)
		{
			if (product.Title is null || product.Title.Length == 0)
			{
				message = "Naslov mora imati barem jedan karakter";
				validRequest = false;
			}
			else if (product.Image is null || product.Image.Length == 0)
			{
				message = "Proizvod mora imati sliku";
				validRequest = false;
			}
			else if (product.ArticleUrl is null || product.ArticleUrl.Length == 0)
			{
				message = "Morate proslijediti odgovarajući link za proizvod sa DMove stranice";
				validRequest = false;
			}
			else
			{
				validRequest = true;
				message = string.Empty;
			}
		}
	}
}
