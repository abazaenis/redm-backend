namespace Redm_backend.Controllers
{
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Models;
	using Redm_backend.Services.ApiKeyService;
	using Redm_backend.Services.CronService;

	using Swashbuckle.AspNetCore.Annotations;

	[ApiController]
	[Route("api/[controller]")]
	public class CronController : ControllerBase
	{
		private readonly ICronService _cronService;

		public CronController(ICronService cronService)
		{
			_cronService = cronService;
		}

		[ApiKey]
		[HttpDelete("DeleteOldPeriods")]
		[SwaggerOperation(Description = "This endpoint is intended exclusively for use by cron jobs. An API key is required to access this endpoint.")]
		public async Task<ActionResult<ServiceResponse<object?>>> DeleteOldPeriods()
		{
			var response = await _cronService.DeleteOldPeriods();

			return Ok(response);
		}
	}
}
