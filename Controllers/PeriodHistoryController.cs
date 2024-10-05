namespace Redm_backend.Controllers
{
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	using Redm_backend.Dtos.PeriodHistory;
	using Redm_backend.Models;
	using Redm_backend.Services.PeriodService;

	using Swashbuckle.AspNetCore.Annotations;

	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class PeriodHistoryController : ControllerBase
	{
		private readonly IPeriodHistoryService _periodService;

		public PeriodHistoryController(IPeriodHistoryService periodService)
		{
			_periodService = periodService;
		}

		[HttpPost("Sync")]
		[SwaggerOperation(Description = "The 'action' field must be 'Add' or 'Delete' and hours, minutes and seconds in 'date' should be 0.")]
		public async Task<ActionResult<ServiceResponse<object?>>> Sync(List<DateActionDto> actions)
		{
			var response = await _periodService.Sync(actions);

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			return Ok(response);
		}

		[HttpPost("AddPeriod")]
		public async Task<ActionResult<ServiceResponse<object?>>> AddPeriod(AddPeriodDto period)
		{
			var response = await _periodService.AddPeriod(period);

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			return Created(string.Empty, response);
		}

		[HttpGet("GetPeriodsAndPredictions")]
		public async Task<ActionResult<ServiceResponse<Dictionary<string, GetPeriodDto>>>> GetPeriodsAndPredictions()
		{
			var response = await _periodService.GetPeriodsAndPredictions();

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			return Ok(response);
		}

		[HttpPatch("UpdatePeriod")]
		public async Task<ActionResult<ServiceResponse<object?>>> UpdatePeriod(UpdatePeriodDto period)
		{
			var response = await _periodService.UpdatePeriod(period);

			if (response.StatusCode == 400)
			{
				return BadRequest(response);
			}

			if (response.StatusCode == 404)
			{
				return NotFound(response);
			}

			return Ok(response);
		}

		[HttpDelete("DeletePeriod")]
		public async Task<ActionResult<ServiceResponse<object?>>> DeletePeriod(int periodId)
		{
			var response = await _periodService.DeletePeriod(periodId);

			if (response.StatusCode == 404)
			{
				return BadRequest(response);
			}

			return Ok(response);
		}
	}
}