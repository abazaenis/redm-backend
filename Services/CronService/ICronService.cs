namespace Redm_backend.Services.CronService
{
	using Redm_backend.Models;

	public interface ICronService
	{
		Task<ServiceResponse<object?>> DeleteOldPeriods();
	}
}
