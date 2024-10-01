namespace Redm_backend.Controllers
{
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Dtos.Post;
	using Redm_backend.Dtos.Product;
	using Redm_backend.Models;
	using Redm_backend.Services.ProductService;

	using Swashbuckle.AspNetCore.Annotations;

	[ApiController]
	[Route("api/[controller]")]
	public class ProductController : ControllerBase
	{
		private readonly IProductService _productService;

		public ProductController(IProductService productService)
		{
			_productService = productService;
		}

		[HttpPost("AddProduct")]
		[SwaggerOperation(Summary = "Requires admin privileges")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ServiceResponse<object?>>> AddProduct(AddProductDto product)
		{
			var response = await _productService.AddProduct(product);

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			if (response.StatusCode == 404)
			{
				return NotFound(response);
			}

			return Created(string.Empty, response);
		}

		[HttpGet("GetProductsGroupedByCategories")]
		[Authorize(Roles = "User,Admin")]
		public async Task<ActionResult<ServiceResponse<List<GetProductsDto>>>> GetProductsGroupedByCategories()
		{
			var response = await _productService.GetProductsGroupedByCategories();

			return Ok(response);
		}

		[HttpDelete("DeleteStory")]
		[SwaggerOperation(Summary = "Requires admin privileges")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ServiceResponse<object?>>> DeleteProduct(int productId)
		{
			var response = await _productService.DeleteProduct(productId);

			if (response.StatusCode == 404)
			{
				return NotFound(response);
			}

			return Ok(response);
		}
	}
}
