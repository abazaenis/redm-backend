namespace Redm_backend.Services.PeriodService
{
    using Redm_backend.Dtos.PeriodHistory;
    using Redm_backend.Models;

    public interface IPeriodHistoryService
    {
        Task<ServiceResponse<object?>> AddPeriod(AddPeriodDto period);

        Task<ServiceResponse<Dictionary<string, GetPeriodDto>>> GetPeriodsByYearByMonth(string yearMonth);

        Task<ServiceResponse<object?>> UpdatePeriod(UpdatePeriodDto period);

        Task<ServiceResponse<object?>> DeletePeriod(int periodId);
    }
}