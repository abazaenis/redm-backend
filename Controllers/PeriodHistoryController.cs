namespace Redm_backend.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Redm_backend.Dtos.PeriodHistory;
    using Redm_backend.Models;
    using Redm_backend.Services.PeriodService;

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