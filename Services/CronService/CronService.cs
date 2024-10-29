namespace Redm_backend.Services.CronService
{
	using System.Text;

	using Azure.Storage.Blobs;
	using Azure.Storage.Blobs.Specialized;

	using Expo.Server.Client;
	using Expo.Server.Models;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.Cron;
	using Redm_backend.Models;

	public class CronService : ICronService
	{
		private readonly DataContext _context;
		private readonly BlobServiceClient _blobServiceClient;
		private readonly PushApiClient _expoSDKClient;

		public CronService(DataContext context, BlobServiceClient blobServiceClient)
		{
			_context = context;
			_blobServiceClient = blobServiceClient;
			_expoSDKClient = new PushApiClient();
		}

		public async Task<ServiceResponse<List<string>?>> AndroidProbationPeriodNotifications()
		{
			var response = new ServiceResponse<List<string>?>();

			var expoPushTokens = await _context.Users
				.Where(u => !string.IsNullOrEmpty(u.ExpoPushToken))
				.Select(user => user.ExpoPushToken)
				.ToListAsync();

			if (expoPushTokens.Count == 0)
			{
				response.DebugMessage = "Nema korisnika kojima je dodijeljen Expo Token.";
				return response;
			}

			var pushTicketRequest = new PushTicketRequest()
			{
				PushTo = new List<string> { "ExponentPushToken[JTHE24HTsNpUYwtHSgGbFe]" }, // expoPushTokens
				PushBadgeCount = 7,
				PushBody = "Samo mali podsjetnik – otvori REDm i ostani u toku! 💕.",
			};

			var result = await _expoSDKClient.PushSendAsync(pushTicketRequest);

			CheckNotificationPushResultErrors(result, response);

			return response;
		}

		public async Task<ServiceResponse<List<string>?>> SendDailyNotifications()
		{
			var response = new ServiceResponse<List<string>?>();
			var ticketIds = new List<string>();

			var usersWithDbEntries = await GetUsersWithExpoTokenDbEntries();

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

				var result = await _expoSDKClient.PushSendAsync(pushTicketReqPeriodIn1Day);
				ticketIds.AddRange(result.PushTicketStatuses.Select(ticket => ticket.TicketId));

				CheckNotificationPushResultErrors(result, response);
			}

			if (pushTokensPeriodIn5Days != null && pushTokensPeriodIn5Days.Count != 0)
			{
				var pushTicketReqPeriodIn5Days = new PushTicketRequest()
				{
					PushTo = pushTokensPeriodIn5Days,
					PushBadgeCount = 7,
					PushBody = "CickoMicko: Menstruacija Vam počinje za 5 dana.",
				};

				var result = await _expoSDKClient.PushSendAsync(pushTicketReqPeriodIn5Days);
				ticketIds.AddRange(result.PushTicketStatuses.Select(ticket => ticket.TicketId));

				CheckNotificationPushResultErrors(result, response);
			}

			if (pushTokensOvulationToday != null && pushTokensOvulationToday.Count != 0)
			{
				var pushTicketReqOvulationToday = new PushTicketRequest()
				{
					PushTo = pushTokensOvulationToday,
					PushBadgeCount = 7,
					PushBody = "CickoMicko: Danas Vam je ovulacija.",
				};

				var result = await _expoSDKClient.PushSendAsync(pushTicketReqOvulationToday);
				ticketIds.AddRange(result.PushTicketStatuses.Select(ticket => ticket.TicketId));

				CheckNotificationPushResultErrors(result, response);
			}

			if (pushTokensFertileDaysStartToday != null && pushTokensFertileDaysStartToday.Count != 0)
			{
				var pushTicketReqFertileDaysStartToday = new PushTicketRequest()
				{
					PushTo = pushTokensFertileDaysStartToday,
					PushBadgeCount = 7,
					PushBody = "CickoMicko: Danas Vam počinju plodni dani.",
				};

				var result = await _expoSDKClient.PushSendAsync(pushTicketReqFertileDaysStartToday);
				ticketIds.AddRange(result.PushTicketStatuses.Select(ticket => ticket.TicketId));

				CheckNotificationPushResultErrors(result, response);
			}

			await SaveTicketIdsToBlobAsync(ticketIds);

			return response;
		}

		public async Task<ServiceResponse<object?>> GetAndProcessReceipts()
		{
			var response = new ServiceResponse<object?>();
			var ticketIds = await ReadTicketIdsFromBlobAsync();
			if (ticketIds == null || !ticketIds.Any())
			{
				response.Message = "Text fajl 'notifications-data' ne sadrži nijedan ticketId";
			}

			var pushReceiptReq = new PushReceiptRequest { PushTicketIds = ticketIds };
			var pushReceiptResult = await _expoSDKClient.PushGetReceiptsAsync(pushReceiptReq);

			foreach (var pushReceipt in pushReceiptResult.PushTicketReceipts)
			{
				if (pushReceipt.Value.DeliveryStatus == "error")
				{
					var errorLog = $"TicketId: {pushReceipt.Key}, Error: {pushReceipt.Value.DeliveryStatus}, Message: {pushReceipt.Value.DeliveryMessage}";
					await AppendErrorLogToBlobAsync(errorLog);
				}
			}

			await ClearTicketIdsBlobAsync();

			response.DebugMessage = "Procesiranje Expo Računa je završeno, errori su logovani u 'NotificationErrorsLog.txt' (ukoliko ih je bilo)";
			return response;
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

		private static void CheckNotificationPushResultErrors(PushTicketResponse result, ServiceResponse<List<string>?> response)
		{
			if (result?.PushTicketErrors?.Count > 0)
			{
				int counter = 1;
				foreach (var error in result.PushTicketErrors)
				{
					response.DebugMessage += $"Error number {counter}: {error.ErrorCode} - {error.ErrorMessage}. | ";
					response.StatusCode = 500;
				}
			}
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

		private async Task<List<UserLastPeriodDto>> GetUsersWithExpoTokenDbEntries()
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

		private async Task SaveTicketIdsToBlobAsync(List<string> ticketIds)
		{
			var containerName = "notification-data";
			var blobName = "ExpoTicketIds.txt";
			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

			await blobContainerClient.CreateIfNotExistsAsync();

			var blobClient = blobContainerClient.GetBlobClient(blobName);
			var logEntry = string.Join(Environment.NewLine, ticketIds) + Environment.NewLine;

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logEntry)))
			{
				await blobClient.UploadAsync(stream, overwrite: true);
			}
		}

		private async Task<List<string>> ReadTicketIdsFromBlobAsync()
		{
			var containerName = "notification-data";
			var blobName = "ExpoTicketIds.txt";
			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = blobContainerClient.GetBlobClient(blobName);

			if (await blobClient.ExistsAsync())
			{
				var downloadResponse = await blobClient.DownloadAsync();
				using (var reader = new StreamReader(downloadResponse.Value.Content))
				{
					var fileContent = await reader.ReadToEndAsync();
					return fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
				}
			}

			return new List<string>();
		}

		private async Task AppendErrorLogToBlobAsync(string errorLog)
		{
			var containerName = "notification-data";
			var errorLogBlobName = "NotificationErrorsLog.txt";
			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

			var appendBlobClient = blobContainerClient.GetAppendBlobClient(errorLogBlobName);

			if (!await appendBlobClient.ExistsAsync())
			{
				await appendBlobClient.CreateAsync();
			}

			var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {errorLog}{Environment.NewLine}";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logEntry)))
			{
				await appendBlobClient.AppendBlockAsync(stream);
			}
		}

		private async Task ClearTicketIdsBlobAsync()
		{
			var containerName = "notification-data";
			var blobName = "ExpoTicketIds.txt";
			var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = blobContainerClient.GetBlobClient(blobName);

			await blobClient.DeleteIfExistsAsync();
		}
	}
}
