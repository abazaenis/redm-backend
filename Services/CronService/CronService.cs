namespace Redm_backend.Services.CronService
{
	using Expo.Server.Client;
	using Expo.Server.Models;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Models;

	public class CronService : ICronService
	{
		private readonly DataContext _context;
		private readonly PushApiClient _expoSDKClient;

		public CronService(DataContext context)
		{
			_context = context;
			_expoSDKClient = new PushApiClient();
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

		public async Task<ServiceResponse<List<string>?>> SendDailyNotifications()
		{
			var response = new ServiceResponse<List<string>?>();

			var pushTokensPeriodIn5Days = await PeriodNotification5Days();

			if (pushTokensPeriodIn5Days == null || !pushTokensPeriodIn5Days.Any())
			{
				response.DebugMessage = "Nema korisnika kojima menstruacija za 5 dana.";
			}

			//response.Data = pushTokensPeriodIn5Days;
			//return response;

			var pushTicketReq = new PushTicketRequest()
			{
				PushTo = pushTokensPeriodIn5Days,
				PushBadgeCount = 7,
				PushBody = Convert.ToString(DateTime.UtcNow),
			};

			var result = await _expoSDKClient.PushSendAsync(pushTicketReq);
			var responseData = new List<string>();

			if (result?.PushTicketErrors?.Any() == true)
			{
				foreach (var error in result.PushTicketErrors)
				{
					responseData.Add($"Error: {error.ErrorCode} - {error.ErrorMessage}");
				}

				response.Data = responseData;
			}

			return response;
		}

		private async Task<List<string>> PeriodNotification5Days()
		{
			var usersWithLastPeriod = await _context.Users
				.Where(u => !string.IsNullOrEmpty(u.ExpoPushToken))
				.Select(u => new
				{
					User = u,
					LastPeriod = _context.PeriodHistory
						.Where(p => p.UserId == u.Id)
						.OrderByDescending(p => p.StartDate)
						.FirstOrDefault(),
				})
				.Where(x => x.LastPeriod != null)
				.ToListAsync();

			var targetDate = DateTime.UtcNow.Date.AddDays(5);
			var userExpoTokensWithPeriodIn5Days = usersWithLastPeriod
				.Where(x =>
				{
					var predictedPeriodStart = x.LastPeriod!.StartDate;
					while (predictedPeriodStart < targetDate)
					{
						predictedPeriodStart = predictedPeriodStart.AddDays(x.User.CycleDuration);
					}

					return predictedPeriodStart == targetDate;
				})
				.Select(x => x.User.ExpoPushToken)
				.ToList();

			return userExpoTokensWithPeriodIn5Days;
		}
	}
}
