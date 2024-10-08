namespace Redm_backend.Services.CronService
{
	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Models;

	public class CronService : ICronService
	{
		private readonly DataContext _context;

		public CronService(DataContext context)
		{
			_context = context;
		}

		public async Task<ServiceResponse<object?>> DeleteOldPeriods()
		{
			var response = new ServiceResponse<object?>();

			var now = DateTime.UtcNow;
			var dateDeletionThreshold = new DateTime(now.AddYears(-1).Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

			var oldPeriods = await _context.PeriodHistory.Where(p => p.EndDate < dateDeletionThreshold).ToListAsync();

			if (oldPeriods.Any())
			{
				_context.PeriodHistory.RemoveRange(oldPeriods);

				await _context.SaveChangesAsync();

				response.DebugMessage = $"Uspješno obrisano {oldPeriods.Count} starih zapisa o periodima.";
			}
			else
			{
				response.DebugMessage = "Nema starih zapisa o periodima za brisanje.";
			}

			return response;
		}
	}
}
