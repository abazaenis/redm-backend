namespace Redm_backend.Controllers
{
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Dtos.HomePage;
	using Redm_backend.Dtos.PeriodHistory;
	using Redm_backend.Models;
	using Redm_backend.Services.HomePageService;

	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class HomePageController : ControllerBase
	{
		private readonly IHomePageService _homePageService;

		public HomePageController(IHomePageService homePageService)
        {
            _homePageService = homePageService;
        }

		[HttpGet("LoadData")]
		public async Task<ActionResult<ServiceResponse<HomePageDataDto>>> LoadData()
		{
			var response = await _homePageService.LoadData();

			return Ok(response);
		}
	}
}
