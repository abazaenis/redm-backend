namespace Redm_backend.Services.PeriodService
{
	using Redm_backend.Dtos.PeriodHistory;
	using Redm_backend.Models;

	public interface IPeriodHistoryService
	{
		Task<ServiceResponse<object?>> Sync(List<DateActionDto> actions);

		Task<ServiceResponse<object?>> AddPeriod(AddPeriodDto period);

		Task<ServiceResponse<Dictionary<string, GetPeriodDto>>> GetPeriodsAndPredictions();

		Task<ServiceResponse<object?>> UpdatePeriod(UpdatePeriodDto period);

		Task<ServiceResponse<object?>> DeletePeriod(int periodId);
	}
}