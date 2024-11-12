namespace Redm_backend.Services.SymptomService
{
	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.Symptom;
	using Redm_backend.Models;
	using Redm_backend.Services.UserService;

	public class SymptomService : ISymptomService
	{
		private readonly DataContext _context;
		private readonly IUserService _userService;

		public SymptomService(DataContext context, IUserService userService)
		{
			_context = context;
			_userService = userService;
		}

		public async Task<ServiceResponse<object?>> AddSymptom(SymptomDto newSymptom)
		{
			var response = new ServiceResponse<object?>();
			var createdSymptom = new Symptom()
			{
				UserId = _userService.GetUserId(),
				Date = newSymptom.Date,
				PhysicalSymptoms = newSymptom.PhysicalSymptoms.Symptoms,
				MoodSymptoms = newSymptom.MoodSymptoms.Symptoms,
				SexualActivitySymptoms = newSymptom.SexualActivitySymptoms.Symptoms,
				OtherSymptoms = newSymptom.OtherSymptoms.Symptoms,
			};

			_context.Symptoms.Add(createdSymptom);
			await _context.SaveChangesAsync();

			response.Data = null;
			response.Message = "Uspješno ste dodali simptome.";

			return response;
		}

		public async Task<ServiceResponse<GetSymptomDto>> GetSymptomByUserByDate(DateTime date)
		{
			var response = new ServiceResponse<GetSymptomDto>();
			var symptoms = await _context.Symptoms.FirstOrDefaultAsync(s => s.UserId == _userService.GetUserId() && s.Date.Date == date.Date.Date);

			if (symptoms is null)
			{
				response.Data = null;
				response.DebugMessage = $"Ne postoje simptomi za korisnika sa id-em {_userService.GetUserId()} na datum {date.Date.ToString("dd/mm/yyyy")}";

				return response;
			}

			var responseData = new GetSymptomDto()
			{
				Id = symptoms.Id,
				Date = date.Date.Date,
				PhysicalSymptoms = new GroupedSymptomsDto { Name = "Simptomi", Symptoms = symptoms.PhysicalSymptoms },
				MoodSymptoms = new GroupedSymptomsDto { Name = "Raspoloženje", Symptoms = symptoms.MoodSymptoms },
				SexualActivitySymptoms = new GroupedSymptomsDto { Name = "Seksualna aktivnost", Symptoms = symptoms.SexualActivitySymptoms },
				OtherSymptoms = new GroupedSymptomsDto { Name = "Ostalo", Symptoms = symptoms.OtherSymptoms },
			};

			response.Data = responseData;

			return response;
		}

		public async Task<ServiceResponse<object?>> UpdateSymptom(SymptomDto newSymptom)
		{
			var response = new ServiceResponse<object?>();

			var existingSymptoms = await _context.Symptoms.FirstOrDefaultAsync(s => s.UserId == _userService.GetUserId() && s.Date.Date == newSymptom.Date.Date);

			if (existingSymptoms is null)
			{
				response.StatusCode = 404;
				response.Data = null;
				response.Message = "Trenutno nismo u mogućnosti da ažuriramo vaše simptome.";
				response.DebugMessage = "Ne postoje simptomi sa datim datumom za trenutnog korisnika";

				return response;
			}

			existingSymptoms.PhysicalSymptoms = newSymptom.PhysicalSymptoms.Symptoms;
			existingSymptoms.MoodSymptoms = newSymptom.MoodSymptoms.Symptoms;
			existingSymptoms.SexualActivitySymptoms = newSymptom.SexualActivitySymptoms.Symptoms;
			existingSymptoms.OtherSymptoms = newSymptom.OtherSymptoms.Symptoms;
			await _context.SaveChangesAsync();

			response.Data = null;
			response.Message = "Uspješno ste ažurirali simptome";

			return response;
		}

		public async Task<ServiceResponse<object?>> DeleteSymptom(int symptomToDelete)
		{
			var response = new ServiceResponse<object?>();
			var symptom = await _context.Symptoms.FirstOrDefaultAsync(s => s.UserId == _userService.GetUserId() && s.Id == symptomToDelete);

			if (symptom is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = "Ne postoji simptom datog korisnika sa datim ID-om";
				return response;
			}

			_context.Symptoms.Remove(symptom);
			await _context.SaveChangesAsync();

			response.Data = null;
			response.DebugMessage = "Uspješno ste obrisali simptome";

			return response;
		}
	}
}