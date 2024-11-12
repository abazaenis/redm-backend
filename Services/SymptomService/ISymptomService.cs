namespace Redm_backend.Services.SymptomService
{
	using Redm_backend.Dtos.Symptom;
	using Redm_backend.Models;

	public interface ISymptomService
	{
		Task<ServiceResponse<object?>> AddSymptom(SymptomDto newSymptom);

		Task<ServiceResponse<GetSymptomDto>> GetSymptomByUserByDate(DateTime date);

		Task<ServiceResponse<object?>> UpdateSymptom(SymptomDto newSymptom);

		Task<ServiceResponse<object?>> DeleteSymptom(int symptomToDelete);
	}
}