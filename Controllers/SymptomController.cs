namespace Redm_backend.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Redm_backend.Dtos.Symptom;
    using Redm_backend.Models;
    using Redm_backend.Services.SymptomService;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SymptomController : ControllerBase
    {
        private readonly ISymptomService _symptomService;

        public SymptomController(ISymptomService symptomService)
        {
            _symptomService = symptomService;
        }

        [HttpPost("AddSymptom")]
        public async Task<ActionResult<ServiceResponse<object?>>> AddSymptom(SymptomDto newSymptom)
        {
            return Created(string.Empty, await _symptomService.AddSymptom(newSymptom));
        }

        [HttpGet("GetSymptomByDate")]
        public async Task<ActionResult<ServiceResponse<GetSymptomDto>>> GetSymptomByUserByDate(DateTime date)
        {
            var response = await _symptomService.GetSymptomByUserByDate(date);

            return Ok(response);
        }

        [HttpPut("UpdateSymptom")]
        public async Task<ActionResult<ServiceResponse<object?>>> UpdateSymptom(SymptomDto newSymptom)
        {
            var response = await _symptomService.UpdateSymptom(newSymptom);
            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpDelete("DeleteSymptom")]
        public async Task<ActionResult<ServiceResponse<object?>>> DeleteSymptom(int symptomToDelete)
        {
            var response = await _symptomService.DeleteSymptom(symptomToDelete);
            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}