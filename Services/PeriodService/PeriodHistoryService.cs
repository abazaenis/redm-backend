namespace Redm_backend.Services.PeriodService
{
	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.PeriodHistory;
	using Redm_backend.Models;
	using Redm_backend.Services.UserService;

	public class PeriodHistoryService : IPeriodHistoryService
	{
		private readonly DataContext _context;
		private readonly IUserService _userService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public PeriodHistoryService(DataContext context, IUserService userService, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_userService = userService;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ServiceResponse<object?>> AddPeriod(AddPeriodDto period)
		{
			var response = new ServiceResponse<object?>();
			var startDate = period.StartDate.Date;
			var endDate = period.EndDate.Date;

			CheckPeriodRequestDates(startDate, endDate, out string message, out bool validDates);

			if (!validDates)
			{
				response.StatusCode = 400;
				response.DebugMessage = message;
				return response;
			}

			var overlapExists = await DetectOverlap(startDate, endDate, _userService.GetUserId());

			if (overlapExists)
			{
				response.StatusCode = 400;
				response.DebugMessage = "Postoji overlap između novog i nekog od starih datuma";
				return response;
			}

			_context.PeriodHistory.Add(new PeriodHistory
			{
				UserId = _userService.GetUserId(),
				StartDate = startDate,
				EndDate = endDate,
			});
			await _context.SaveChangesAsync();

			response.StatusCode = 201;
			response.DebugMessage = "Uspješno ste dodali novu menstruaciju";
			return response;
		}

		public async Task<ServiceResponse<Dictionary<string, GetPeriodDto>>> GetPeriodsAndPredictions()
		{
			// Variables
			var response = new ServiceResponse<Dictionary<string, GetPeriodDto>>();
			var periodsDictionary = new Dictionary<string, GetPeriodDto>();

			var averageCycleLength = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("CycleDuration")?.Value ?? "28");
			var averagePeriodLength = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("PeriodDuration")?.Value ?? "5");
			var userId = _userService.GetUserId();

			await AddPeriodDays(periodsDictionary, userId);

			// Add predicted periods based on database entries
			var lastPeriod = await _context.PeriodHistory
				.Where(ph => ph.UserId == userId)
				.OrderByDescending(ph => ph.EndDate)
				.FirstOrDefaultAsync();

			if (lastPeriod != null)
			{
				var predictedStartDate = lastPeriod.StartDate.AddDays(averageCycleLength);

				AddPredictedPeriodDays(ref periodsDictionary, averageCycleLength, averagePeriodLength, predictedStartDate);
			}

			if (!periodsDictionary.Any())
			{
				response.Data = periodsDictionary;
				return response;
			}

			var periods = new List<(DateTime startDate, DateTime endDate)>();
			SeparatePeriods(ref periods, periodsDictionary);

			HandleOvulationAndFertileDays(periodsDictionary, periods, averageCycleLength);

			response.Data = periodsDictionary;
			return response;
		}

		public async Task<ServiceResponse<object?>> UpdatePeriod(UpdatePeriodDto period)
		{
			var response = new ServiceResponse<object?>();
			var startDate = period.StartDate.Date;
			var endDate = period.EndDate.Date;

			CheckPeriodRequestDates(startDate, endDate, out string message, out bool validDates);

			if (!validDates)
			{
				response.StatusCode = 400;
				response.DebugMessage = message;
				return response;
			}

			var overlapExists = await DetectOverlap(startDate, endDate, _userService.GetUserId(), true, period.PeriodId);

			if (overlapExists)
			{
				response.StatusCode = 400;
				response.DebugMessage = "Postoji overlap između novog i nekog od starih datuma";
				return response;
			}

			var periodDb = await _context.PeriodHistory.FirstOrDefaultAsync(ph => (ph.Id == period.PeriodId) && (ph.UserId == _userService.GetUserId()));

			if (periodDb == null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Period sa id-om {period.PeriodId} ne postoji";
				return response;
			}

			periodDb.StartDate = startDate;
			periodDb.EndDate = endDate;
			await _context.SaveChangesAsync();

			response.DebugMessage = "Uspješno ste ažurirali menstruaciju";
			return response;
		}

		public async Task<ServiceResponse<object?>> DeletePeriod(int periodId)
		{
			var response = new ServiceResponse<object?>();

			var period = await _context.PeriodHistory.FirstOrDefaultAsync(ph => (ph.Id == periodId) && (ph.UserId == _userService.GetUserId()));

			if (period is null)
			{
				response.StatusCode = 404;
				response.DebugMessage = $"Ne postoji menstruacija sa id-om {periodId}";
				return response;
			}

			_context.PeriodHistory.Remove(period);
			await _context.SaveChangesAsync();

			response.DebugMessage = "Uspješno ste obrisali menstruaciju";
			return response;
		}

		private async Task<bool> DetectOverlap(DateTime startDate, DateTime endDate, int userId, bool update = false, int periodId = -1)
		{
			var overlapExists = false;

			if (update)
			{
				overlapExists = await _context.PeriodHistory.AnyAsync(
									ph => ph.UserId == userId && ph.Id != periodId && (
									(startDate >= ph.StartDate && startDate <= ph.EndDate) ||
									(endDate >= ph.StartDate && endDate <= ph.EndDate) ||
									(startDate <= ph.StartDate && endDate >= ph.EndDate)));
			}
			else
			{
				overlapExists = await _context.PeriodHistory.AnyAsync(
									ph => ph.UserId == userId && (
									(startDate >= ph.StartDate && startDate <= ph.EndDate) ||
									(endDate >= ph.StartDate && endDate <= ph.EndDate) ||
									(startDate <= ph.StartDate && endDate >= ph.EndDate)));
			}

			return overlapExists;
		}

		private void CheckPeriodRequestDates(DateTime startDate, DateTime endDate, out string message, out bool validRequest)
		{
			startDate = startDate.Date;
			endDate = endDate.Date;

			if (endDate < startDate)
			{
				validRequest = false;
				message = "Datum završetka ne može biti veći od datuma kraja";
			}
			else if (endDate - startDate > TimeSpan.FromDays(10))
			{
				validRequest = false;
				message = "Trajanje menstruacije može biti najmanje 1 dan i najviše 10 dana.";
			}
			else
			{
				message = string.Empty;
				validRequest = true;
			}
		}

		private async Task AddPeriodDays(Dictionary<string, GetPeriodDto> periodsDictionary, int userId)
		{
			// Add actual periods from database
			var periodHistory = await _context.PeriodHistory
				.Where(ph => ph.UserId == userId)
				.OrderBy(ph => ph.StartDate)
				.ToListAsync();

			foreach (var period in periodHistory)
			{
				var startDate = DateTime.SpecifyKind(period.StartDate, DateTimeKind.Utc);
				var endDate = DateTime.SpecifyKind(period.EndDate, DateTimeKind.Utc);

				for (var date = startDate; date <= endDate; date = date.AddDays(1))
				{
					var periodDto = new GetPeriodDto
					{
						Id = period.Id,
						Selected = true,
						Color = CalendarColor.Period,
						TextColor = "#000",
						StartingDay = date == startDate,
						EndingDay = date == endDate,
					};

					periodsDictionary[date.ToString("yyyy-MM-dd")] = periodDto;
				}
			}
		}

		private void AddPredictedPeriodDays(ref Dictionary<string, GetPeriodDto> periodsDictionary, int averageCycleLength, int averagePeriodLength, DateTime predictedStartDate)
		{
			var endDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
				   .AddMonths(1)
				   .AddYears(2);

			while (predictedStartDate <= endDate)
			{
				var predictedEndDate = predictedStartDate.AddDays(averagePeriodLength - 1);

				for (var date = predictedStartDate; date <= predictedEndDate; date = date.AddDays(1))
				{
					var dateKey = date.ToString("yyyy-MM-dd");

					if (!periodsDictionary.ContainsKey(dateKey))
					{
						var periodDto = new GetPeriodDto
						{
							Id = -1,
							Selected = false,
							Color = CalendarColor.Prediction,
							TextColor = "#000",
							StartingDay = date == predictedStartDate,
							EndingDay = date == predictedEndDate,
						};

						periodsDictionary[dateKey] = periodDto;
					}
				}

				predictedStartDate = predictedStartDate.AddDays(averageCycleLength);
			}
		}

		private void AddOvulationAndFertileDays(ref Dictionary<string, GetPeriodDto> dict, DateTime fertileStart, DateTime fertileEnd)
		{
			var ovulationDate = fertileStart.AddDays(5);

			var ovulationDateKey = ovulationDate.ToString("yyyy-MM-dd");
			if (!dict.ContainsKey(ovulationDateKey)) 
			{
				dict[ovulationDateKey] = new GetPeriodDto
				{
					Id = -2,
					Selected = false,
					Color = CalendarColor.Ovulation,
					TextColor = "#fff",
					StartingDay = true,
					EndingDay = true,
				};
			}

			for (var date = fertileStart; date <= fertileEnd; date = date.AddDays(1))
			{
				var dateKey = date.ToString("yyyy-MM-dd");
				if (!dict.ContainsKey(dateKey)) 
				{
					dict[dateKey] = new GetPeriodDto
					{
						Id = -3,
						Selected = false,
						Color = CalendarColor.Fertile,
						TextColor = "#fff",
						StartingDay = date == fertileStart,
						EndingDay = date == fertileEnd,
					};
				}
			}
		}

		private void HandleOvulationAndFertileDays(Dictionary<string, GetPeriodDto> periodsDictionary, List<(DateTime startDate, DateTime endDate)> periods, int averageCycleLength)
		{
			for (int i = 0; i < periods.Count; i++)
			{
				var currentPeriodStart = DateTime.SpecifyKind(periods[i].startDate, DateTimeKind.Utc);
				var currentPeriodEnd = DateTime.SpecifyKind(periods[i].endDate, DateTimeKind.Utc);

				if (i == 0)
				{
						var ovulationDate = currentPeriodStart.AddDays(-14);
						var fertileStartDate = ovulationDate.AddDays(-5);
						var fertileEndDate = ovulationDate.AddDays(1);
						AddOvulationAndFertileDays(ref periodsDictionary, fertileStartDate, fertileEndDate);
				}

				if (i > 0)
				{
					var previousPeriodStart = DateTime.SpecifyKind(periods[i - 1].startDate, DateTimeKind.Utc);
					var previousPeriodEnd = DateTime.SpecifyKind(periods[i - 1].endDate, DateTimeKind.Utc);

					if ((currentPeriodStart - previousPeriodEnd).TotalDays >= 19)
					{
						var ovulationDate = currentPeriodStart.AddDays(-14);
						var fertileStartDate = ovulationDate.AddDays(-5);
						var fertileEndDate = ovulationDate.AddDays(1);
						AddOvulationAndFertileDays(ref periodsDictionary, fertileStartDate, fertileEndDate);
					}
				}

				if (i == periods.Count - 1)
				{

						var nextPeriodStart = currentPeriodStart.AddDays(averageCycleLength);
						var nextOvulationDate = nextPeriodStart.AddDays(-14);
						var nextFertileStartDate = nextOvulationDate.AddDays(-5);
						var nextFertileEndDate = nextOvulationDate.AddDays(1);
						AddOvulationAndFertileDays(ref periodsDictionary, nextFertileStartDate, nextFertileEndDate);

				}
			}
		}

		private void SeparatePeriods(ref List<(DateTime startDate, DateTime endDate)> periods, Dictionary<string, GetPeriodDto> periodsDictionary)
		{
			var sortedDates = periodsDictionary.Keys
				.Select(date => DateTime.Parse(date))
				.OrderBy(date => date)
				.ToList();

			DateTime? periodStart = sortedDates.First();

			for (int i = 1; i < sortedDates.Count; i++)
			{
				var current = sortedDates[i];
				var previous = sortedDates[i - 1];

				if ((current - previous).TotalDays > 1)
				{
					periods.Add((periodStart.Value, previous));
					periodStart = current;
				}
			}

			if (periodStart.HasValue)
			{
				periods.Add((periodStart.Value, sortedDates.Last()));
			}
		}
	}
}