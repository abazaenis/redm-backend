﻿namespace Redm_backend.Services.CronService
{
	using Expo.Server.Client;
	using Expo.Server.Models;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.Cron;
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

			var usersWithDbEntries = await GetUsersWithDbEntries();

			var pushTokensPeriodIn1Day = GetUsersWithPeriodIn1Day(usersWithDbEntries);
			var pushTokensPeriodIn5Days = GetUsersWithPeriodIn5Days(usersWithDbEntries);
			var pushTokensOvulationToday = GetUsersWithOvulationToday(usersWithDbEntries);
			var pushTokensFertileDaysStartToday = GetUsersWithFertileDaysStartToday(usersWithDbEntries);

			if ((pushTokensPeriodIn5Days == null || !pushTokensPeriodIn5Days.Any()) &&
				(pushTokensPeriodIn1Day == null || !pushTokensPeriodIn1Day.Any()) &&
				(pushTokensOvulationToday == null || !pushTokensOvulationToday.Any()) &&
				(pushTokensFertileDaysStartToday == null || !pushTokensFertileDaysStartToday.Any()))
			{
				response.DebugMessage = "Nema korisnika kojima menstruacija za 5 dana ili dan, ni kojima je ovulacija ili početak plodnih dana danas.";
				return response;
			}

			if (pushTokensPeriodIn1Day != null && pushTokensPeriodIn1Day.Count != 0)
			{
				var pushTicketReqPeriodIn1Day = new PushTicketRequest()
				{
					PushTo = pushTokensPeriodIn1Day,
					PushBadgeCount = 7,
					PushBody = "CickoMicko: Menstruacija Vam počinje sutra.",
				};

				await _expoSDKClient.PushSendAsync(pushTicketReqPeriodIn1Day);
			}

			if (pushTokensPeriodIn5Days != null && pushTokensPeriodIn5Days.Count != 0)
			{
				var pushTicketReqPeriodIn5Days = new PushTicketRequest()
				{
					PushTo = pushTokensPeriodIn5Days,
					PushBadgeCount = 7,
					PushBody = "CickoMicko: Menstruacija Vam počinje za 5 dana.",
				};

				await _expoSDKClient.PushSendAsync(pushTicketReqPeriodIn5Days);
			}

			if (pushTokensOvulationToday != null && pushTokensOvulationToday.Count != 0)
			{
				var pushTicketReqOvulationToday = new PushTicketRequest()
				{
					PushTo = pushTokensOvulationToday,
					PushBadgeCount = 7,
					PushBody = "CickoMicko: Danas Vam je ovulacija.",
				};

				await _expoSDKClient.PushSendAsync(pushTicketReqOvulationToday);
			}

			if (pushTokensFertileDaysStartToday != null && pushTokensFertileDaysStartToday.Count != 0)
			{
				var pushTicketReqFertileDaysStartToday = new PushTicketRequest()
				{
					PushTo = pushTokensFertileDaysStartToday,
					PushBadgeCount = 7,
					PushBody = "CickoMicko: Danas Vam počinju plodni dani.",
				};

				await _expoSDKClient.PushSendAsync(pushTicketReqFertileDaysStartToday);
			}

			return response;
		}

		private static List<string> GetUsersWithPeriodIn1Day(List<UserLastPeriodDto> usersWithDbEntries)
		{
			var targetDateIn1Day = DateTime.UtcNow.Date.AddDays(1);
			var userExpoTokensWithPeriodIn1Day = usersWithDbEntries
				.Where(x =>
				{
					var predictedPeriodStart = x.LastPeriod!.StartDate;
					while (predictedPeriodStart < targetDateIn1Day)
					{
						predictedPeriodStart = predictedPeriodStart.AddDays(x.User.CycleDuration);
					}

					return predictedPeriodStart == targetDateIn1Day;
				})
				.Select(x => x.User.ExpoPushToken)
				.ToList();

			return userExpoTokensWithPeriodIn1Day;
		}

		private static List<string> GetUsersWithPeriodIn5Days(List<UserLastPeriodDto> usersWithDbEntries)
		{
			var targetDateIn5Days = DateTime.UtcNow.Date.AddDays(5);
			var userExpoTokensWithPeriodIn5Days = usersWithDbEntries
				.Where(x =>
				{
					var predictedPeriodStart = x.LastPeriod!.StartDate;
					while (predictedPeriodStart < targetDateIn5Days)
					{
						predictedPeriodStart = predictedPeriodStart.AddDays(x.User.CycleDuration);
					}

					return predictedPeriodStart == targetDateIn5Days;
				})
				.Select(x => x.User.ExpoPushToken)
				.ToList();

			return userExpoTokensWithPeriodIn5Days;
		}

		private static List<string> GetUsersWithOvulationToday(List<UserLastPeriodDto> usersWithDbEntries)
		{
			var today = DateTime.UtcNow.Date;
			var userExpoTokensWithOvulationToday = usersWithDbEntries
				.Where(x =>
				{
					var predictedPeriodStart = x.LastPeriod!.StartDate;
					var predictedOvulationStart = predictedPeriodStart.AddDays(-14);
					while (predictedOvulationStart < today)
					{
						predictedPeriodStart = predictedPeriodStart.AddDays(x.User.CycleDuration);
						predictedOvulationStart = predictedPeriodStart.AddDays(-14);
					}

					return predictedOvulationStart == today;
				})
				.Select(x => x.User.ExpoPushToken)
				.ToList();

			return userExpoTokensWithOvulationToday;
		}

		private static List<string> GetUsersWithFertileDaysStartToday(List<UserLastPeriodDto> usersWithDbEntries)
		{
			var today = DateTime.UtcNow.Date;
			var userExpoTokensWithFertileDayStartingToday = usersWithDbEntries
				.Where(x =>
				{
					var predictedPeriodStart = x.LastPeriod!.StartDate;
					var predictedFertileDaysStart = predictedPeriodStart.AddDays(-14).AddDays(-5);
					while (predictedFertileDaysStart < today)
					{
						predictedPeriodStart = predictedPeriodStart.AddDays(x.User.CycleDuration);
						predictedFertileDaysStart = predictedPeriodStart.AddDays(-14).AddDays(-5);
					}

					return predictedFertileDaysStart == today;
				})
				.Select(x => x.User.ExpoPushToken)
				.ToList();

			return userExpoTokensWithFertileDayStartingToday;
		}

		private async Task<List<UserLastPeriodDto>> GetUsersWithDbEntries()
		{
			var usersWithLastPeriod = await _context.Users
				.Where(u => !string.IsNullOrEmpty(u.ExpoPushToken))
				.Select(u => new UserLastPeriodDto
				{
					User = u,
					LastPeriod = _context.PeriodHistory
						.Where(p => p.UserId == u.Id)
						.OrderByDescending(p => p.StartDate)
						.FirstOrDefault(),
				})
				.Where(x => x.LastPeriod != null)
				.ToListAsync();

			return usersWithLastPeriod;
		}

	}
}
