namespace Redm_backend.Services.HomePageService
{
	using Redm_backend.Dtos.HomePage;
	using Redm_backend.Models;

	public interface IHomePageService
	{
		Task<ServiceResponse<HomePageDataDto>> LoadData();
	}
}
