namespace Redm_backend.Services.CronService
{
	using Redm_backend.Models;

	public interface ICronService
	{
		Task<ServiceResponse<List<string>?>> SendDailyNotifications();

		Task<ServiceResponse<object?>> GetAndProcessReceipts();

		Task<ServiceResponse<object?>> DeleteOldPeriods();
	}
}
