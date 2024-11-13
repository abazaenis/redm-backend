namespace Redm_backend.Services.PeriodService
{
	using System;

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

		public async Task<ServiceResponse<object?>> Sync(List<DateActionDto> actions)
		{
			var response = new ServiceResponse<object?>();

			if (actions is null)
			{
				response.StatusCode = 400;
				response.DebugMessage = "Akcije ne mogu biti null";
				return response;
			}

			if (actions.Count == 0)
			{
				response.StatusCode = 400;
				response.DebugMessage = "Akcije moraju imati makar jednu akciju.";
				return response;
			}

			var userId = _userService.GetUserId();
			var smallestDate = actions.Min(a => a.Date);
			var largestDate = actions.Max(a => a.Date);

			var smallestDateStart = new DateTime(smallestDate.Year, smallestDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
			var largestDateEnd = new DateTime(largestDate.Year, largestDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1);

			var periodsInRange = await _context.PeriodHistory
				.Where(ph => ph.UserId == userId &&
				((ph.StartDate >= smallestDateStart.AddDays(-1) && ph.StartDate <= largestDateEnd.AddDays(1)) ||
				(ph.EndDate >= smallestDateStart.AddDays(-1) && ph.EndDate <= largestDateEnd.AddDays(1))))
				.OrderBy(ph => ph.StartDate)
				.ToListAsync();

			foreach (var action in actions)
			{
				action.Date = new DateTime(action.Date.Year, action.Date.Month, action.Date.Day, 0, 0, 0, 0, DateTimeKind.Utc);

				if (action.Action == ActionType.Add)
				{
					if (periodsInRange.Count == 0)
					{
						var newPeriod = new PeriodHistory
						{
							UserId = userId,
							StartDate = DateTime.SpecifyKind(action.Date, DateTimeKind.Utc),
							EndDate = DateTime.SpecifyKind(action.Date, DateTimeKind.Utc),
						};

						_context.PeriodHistory.Add(newPeriod);
						periodsInRange.Add(newPeriod);
					}
					else
					{
						HandleAddPeriodDay(periodsInRange, action.Date, 0, periodsInRange.Count - 1, response);
					}
				}
				else if (action.Action == ActionType.Delete)
				{
					if (periodsInRange.Count == 0)
					{
						response.StatusCode = 404;
						response.DebugMessage = $"Nema perioda za obrisati na datum: {action.Date}";
						return response;
					}

					HandleDeletePeriodDay(periodsInRange, action.Date, 0, periodsInRange.Count - 1, response);
				}
			}

			if (response.StatusCode == 200)
			{
				await _context.SaveChangesAsync();
			}

			return response;
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

			var lastPeriod = await _context.PeriodHistory
				.Where(ph => ph.UserId == userId)
				.OrderByDescending(ph => ph.EndDate)
				.FirstOrDefaultAsync();

			if (lastPeriod != null)
			{
				var predictedStartDate = lastPeriod.StartDate.AddDays(averageCycleLength);

				AddPredictedPeriodDays(ref periodsDictionary, averageCycleLength, averagePeriodLength, predictedStartDate);
			}

			if (periodsDictionary.Count == 0)
			{
				response.Data = periodsDictionary;
				return response;
			}

			var periods = new List<(DateTime startDate, DateTime endDate)>();
			SeparatePeriods(ref periods, periodsDictionary);

			HandleOvulationAndFertileDays(periodsDictionary, periods, averageCycleLength);

			var sortedPeriodsDictionary = periodsDictionary
				.OrderBy(p => DateTime.Parse(p.Key))
				.ToDictionary(p => p.Key, p => p.Value);

			response.Data = sortedPeriodsDictionary;
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

		private static void CheckPeriodRequestDates(DateTime startDate, DateTime endDate, out string message, out bool validRequest)
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

		private static void SeparatePeriods(ref List<(DateTime startDate, DateTime endDate)> periods, Dictionary<string, GetPeriodDto> periodsDictionary)
		{
			var sortedDates = periodsDictionary.Keys
				.Select(date => DateTime.Parse(date))
				.OrderBy(date => date)
				.ToList();

			DateTime? periodStart = sortedDates[0];

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

			periods.Add((periodStart.Value, sortedDates[^1]));
		}

		private static void AddPredictedPeriodDays(ref Dictionary<string, GetPeriodDto> periodsDictionary, int averageCycleLength, int averagePeriodLength, DateTime predictedStartDate)
		{
			var endDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
				   .AddMonths(1)
				   .AddYears(2);

			while (predictedStartDate <= endDate)
			{
				var predictedEndDate = predictedStartDate.AddDays(averagePeriodLength - 1);
				int counter = 1;

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
							DayIndex = counter,
						};

						periodsDictionary[dateKey] = periodDto;
					}

					counter++;
				}

				predictedStartDate = predictedStartDate.AddDays(averageCycleLength);
			}
		}

		private static void AddOvulationAndFertileDays(ref Dictionary<string, GetPeriodDto> dict, DateTime fertileStart, DateTime fertileEnd)
		{
			var ovulationDate = fertileStart.AddDays(5);

			int counter = 1;
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
						StartingDay = counter == 1,
						EndingDay = date == fertileEnd,
						DayIndex = counter,
					};

					counter++;
				}
			}

			var ovulationDateKey = ovulationDate.ToString("yyyy-MM-dd");
			var ovulationDateTaken = dict.ContainsKey(ovulationDateKey);
			if (!ovulationDateTaken || dict[ovulationDateKey].Id == -3)
			{
				dict[ovulationDateKey] = new GetPeriodDto
				{
					Id = -2,
					Selected = false,
					Color = CalendarColor.Ovulation,
					TextColor = "#fff",
					StartingDay = false,
					EndingDay = false,
					DayIndex = counter - 2,
				};
			}
		}

		private static void HandleOvulationAndFertileDays(Dictionary<string, GetPeriodDto> periodsDictionary, List<(DateTime startDate, DateTime endDate)> periods, int averageCycleLength)
		{
			for (int i = 0; i < periods.Count; i++)
			{
				var currentPeriodStart = DateTime.SpecifyKind(periods[i].startDate, DateTimeKind.Utc);

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

					if ((currentPeriodStart - previousPeriodStart).TotalDays > 19)
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
				int counter = 1;

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
						DayIndex = counter,
					};

					periodsDictionary[date.ToString("yyyy-MM-dd")] = periodDto;
					counter++;
				}
			}
		}

		private void HandleDeletePeriodDay(List<PeriodHistory> periodsInRange, DateTime date, int firstIndex, int secondIndex, ServiceResponse<object?> response)
		{
			if (firstIndex == secondIndex)
			{
				var period = periodsInRange[firstIndex];
				if (date >= period.StartDate && date <= period.EndDate)
				{
					DeletePeriodDay(periodsInRange, period, date);
					return;
				}

				response.DebugMessage = $"Nema perioda za obrisati na datum: {date}";
				response.StatusCode = 404;
				return;
			}

			if (Math.Abs(firstIndex - secondIndex) == 1)
			{
				var firstPeriod = periodsInRange[firstIndex];
				var secondPeriod = periodsInRange[secondIndex];

				if (date >= firstPeriod.StartDate && date <= firstPeriod.EndDate)
				{
					DeletePeriodDay(periodsInRange, firstPeriod, date);
					return;
				}

				if (date >= secondPeriod.StartDate && date <= secondPeriod.EndDate)
				{
					DeletePeriodDay(periodsInRange, secondPeriod, date);
					return;
				}

				response.DebugMessage = $"Nema perioda za obrisati na datum: {date}";
				response.StatusCode = 404;
				return;
			}

			int midIndex = (firstIndex + secondIndex) / 2;
			var midPeriod = periodsInRange[midIndex];

			if (date >= midPeriod.StartDate && date <= midPeriod.EndDate)
			{
				DeletePeriodDay(periodsInRange, midPeriod, date);
				return;
			}

			if (date < midPeriod.StartDate)
			{
				HandleDeletePeriodDay(periodsInRange, date, firstIndex, midIndex - 1, response);
			}
			else
			{
				HandleDeletePeriodDay(periodsInRange, date, midIndex + 1, secondIndex, response);
			}
		}

		private void DeletePeriodDay(List<PeriodHistory> periodsInRange, PeriodHistory period, DateTime date)
		{
			if (date == period.StartDate)
			{
				if (period.StartDate == period.EndDate)
				{
					_context.PeriodHistory.Remove(period);
					periodsInRange.Remove(period);
				}
				else
				{
					period.StartDate = period.StartDate.AddDays(1);
				}
			}
			else if (date == period.EndDate)
			{
				if (period.EndDate == period.StartDate)
				{
					_context.PeriodHistory.Remove(period);
					periodsInRange.Remove(period);
				}
				else
				{
					period.EndDate = period.EndDate.AddDays(-1);
				}
			}
			else
			{
				_context.PeriodHistory.Remove(period);
				periodsInRange.Remove(period);

				var newPeriod1 = new PeriodHistory
				{
					UserId = period.UserId,
					StartDate = DateTime.SpecifyKind(period.StartDate, DateTimeKind.Utc),
					EndDate = DateTime.SpecifyKind(date.AddDays(-1), DateTimeKind.Utc),
				};
				_context.PeriodHistory.Add(newPeriod1);
				periodsInRange.Add(newPeriod1);

				var newPeriod2 = new PeriodHistory
				{
					UserId = period.UserId,
					StartDate = DateTime.SpecifyKind(date.AddDays(1), DateTimeKind.Utc),
					EndDate = DateTime.SpecifyKind(period.EndDate, DateTimeKind.Utc),
				};
				_context.PeriodHistory.Add(newPeriod2);
				periodsInRange.Add(newPeriod2);

				periodsInRange.Sort((p1, p2) => p1.StartDate.CompareTo(p2.StartDate));
			}
		}

		private void HandleAddPeriodDay(List<PeriodHistory> periodsInRange, DateTime date, int firstIndex, int secondIndex, ServiceResponse<object?> response)
		{
			if (firstIndex == secondIndex)
			{
				var period = periodsInRange[firstIndex];
				if (date >= period.StartDate && date <= period.EndDate)
				{
					response.StatusCode = 400;
					response.DebugMessage = $"Postoji overlap sa menstruacijom: {period.StartDate} to {period.EndDate}\n";
					return;
				}

				var previousPeriod = periodsInRange[firstIndex];
				var nextPeriod = periodsInRange[secondIndex];

				if (date > period.StartDate)
				{
					nextPeriod = secondIndex < periodsInRange.Count - 1 ? periodsInRange[secondIndex + 1] : null;
				}
				else
				{
					previousPeriod = firstIndex > 0 ? periodsInRange[firstIndex - 1] : null;
				}

				AddPeriodDay(periodsInRange, previousPeriod!, nextPeriod, date);

				return;
			}

			if (Math.Abs(firstIndex - secondIndex) == 1)
			{
				var firstPeriod = periodsInRange[firstIndex];
				var secondPeriod = periodsInRange[secondIndex];

				if (date >= firstPeriod.StartDate && date <= firstPeriod.EndDate)
				{
					response.DebugMessage += $"Postoji overlap sa menstruacijom: {firstPeriod.StartDate} to {firstPeriod.EndDate}\n";
					return;
				}

				if (date >= secondPeriod.StartDate && date <= secondPeriod.EndDate)
				{
					response.DebugMessage += $"Postoji overlap sa menstruacijom: {secondPeriod.StartDate} to {secondPeriod.EndDate}\n";
					return;
				}

				if (date > firstPeriod.EndDate && date < secondPeriod.StartDate)
				{
					AddPeriodDay(periodsInRange, firstPeriod, secondPeriod, date);
					return;
				}

				if (date < firstPeriod.StartDate)
				{
					secondPeriod = firstPeriod;
					firstPeriod = firstIndex > 0 ? periodsInRange[firstIndex - 1] : null;

					AddPeriodDay(periodsInRange, firstPeriod!, secondPeriod, date);
					return;
				}

				if (date > secondPeriod.EndDate)
				{
					firstPeriod = secondPeriod;
					secondPeriod = secondIndex < periodsInRange.Count - 1 ? periodsInRange[secondIndex + 1] : null;

					AddPeriodDay(periodsInRange, firstPeriod, secondPeriod, date);
					return;
				}

				return;
			}

			int midIndex = (firstIndex + secondIndex) / 2;
			var midPeriod = periodsInRange[midIndex];

			if (date >= midPeriod.StartDate && date <= midPeriod.EndDate)
			{
				response.DebugMessage += $"Postoji overlap sa menstruacijom: {midPeriod.StartDate} to {midPeriod.EndDate}\n";
				return;
			}

			if (date < midPeriod.StartDate)
			{
				HandleAddPeriodDay(periodsInRange, date, firstIndex, midIndex - 1, response);
			}
			else
			{
				HandleAddPeriodDay(periodsInRange, date, midIndex + 1, secondIndex, response);
			}
		}

		private void AddPeriodDay(List<PeriodHistory> periodsInRange, PeriodHistory firstPeriod, PeriodHistory? secondPeriod, DateTime date)
		{
			if (firstPeriod == null && secondPeriod == null)
			{
				var newPeriod = new PeriodHistory
				{
					UserId = _userService.GetUserId(),
					StartDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
					EndDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
				};

				_context.PeriodHistory.Add(newPeriod);
				periodsInRange.Add(newPeriod);
				periodsInRange.Sort((p1, p2) => p1.StartDate.CompareTo(p2.StartDate));
			}
			else if (firstPeriod != null && secondPeriod == null)
			{
				if ((int)(date - firstPeriod.EndDate).TotalDays == 1)
				{
					firstPeriod.EndDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
				}
				else
				{
					var newPeriod = new PeriodHistory
					{
						UserId = _userService.GetUserId(),
						StartDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
						EndDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
					};

					_context.PeriodHistory.Add(newPeriod);
					periodsInRange.Add(newPeriod);
					periodsInRange.Sort((p1, p2) => p1.StartDate.CompareTo(p2.StartDate));
				}
			}
			else if (firstPeriod == null)
			{
				if ((int)(secondPeriod!.StartDate - date).TotalDays == 1)
				{
					secondPeriod.StartDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
				}
				else
				{
					var newPeriod = new PeriodHistory
					{
						UserId = _userService.GetUserId(),
						StartDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
						EndDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
					};

					_context.PeriodHistory.Add(newPeriod);
					periodsInRange.Add(newPeriod);
					periodsInRange.Sort((p1, p2) => p1.StartDate.CompareTo(p2.StartDate));
				}
			}
			else
			{
				if ((date - firstPeriod!.EndDate).TotalDays > 1 && (secondPeriod!.StartDate - date).TotalDays > 1)
				{
					var newPeriod = new PeriodHistory
					{
						UserId = _userService.GetUserId(),
						StartDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
						EndDate = DateTime.SpecifyKind(date, DateTimeKind.Utc),
					};

					_context.PeriodHistory.Add(newPeriod);
					periodsInRange.Add(newPeriod);
					periodsInRange.Sort((p1, p2) => p1.StartDate.CompareTo(p2.StartDate));
				}
				else if ((int)(date - firstPeriod.EndDate).TotalDays == 1 && (int)(secondPeriod!.StartDate - date).TotalDays == 1)
				{
					firstPeriod.EndDate = secondPeriod.EndDate;
					_context.PeriodHistory.Remove(secondPeriod);
					periodsInRange.Remove(secondPeriod);
				}
				else if ((int)(date - firstPeriod.EndDate).TotalDays == 1)
				{
					firstPeriod.EndDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
				}
				else if ((int)(secondPeriod!.StartDate - date).TotalDays == 1)
				{
					secondPeriod.StartDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
				}
			}
		}
	}
}