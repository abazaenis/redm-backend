namespace Redm_backend.Services.ProductService
{
	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.Product;
    using Redm_backend.Models;

    public interface IProductService
	{
		Task<ServiceResponse<object?>> AddProduct(AddProductDto product);

		Task<ServiceResponse<List<GetProductsDto>>> GetProductsGroupedByCategories();

		Task<ServiceResponse<object?>> DeleteProduct(int productId);
	}
}
