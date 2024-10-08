namespace Redm_backend.Services.HomePageService
{
	using System.Security.Claims;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.HomePage;
	using Redm_backend.Models;
	using Redm_backend.Services.PeriodService;

	public class HomePageService : IHomePageService
	{
		private readonly DataContext _context;
		private readonly IPeriodHistoryService _periodHistoryService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public HomePageService(DataContext context, IPeriodHistoryService periodHistoryService, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_periodHistoryService = periodHistoryService;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ServiceResponse<HomePageDataDto>> LoadData()
		{
			var response = new ServiceResponse<HomePageDataDto>();
			var data = new HomePageDataDto();

			var userId = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue("UserId")!);
			var userCycleDuration = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue("CycleDuration")!);
			var lastThreeUserPeriods = await _context.PeriodHistory.Where(ph => ph.UserId == userId)
													 .OrderByDescending(ph => ph.StartDate)
													 .Take(3)
													 .ToListAsync();
			var lastPeriod = lastThreeUserPeriods.FirstOrDefault();
			data.PeriodData = (await _periodHistoryService.GetPeriodsAndPredictions()).Data;

			if (lastPeriod == null)
			{
				data.NextPeriod = null;
				data.NextOvulation = null;
				data.NextFertileDay = null;
				response.Data = data;
				return response;
			}

			CalculateNextPeriodStartDate(data, userCycleDuration, lastPeriod, out DateTime nextPeriod);

			CalculateNextOvulationAndFertileDay(data, nextPeriod, userCycleDuration);

			CalculateAveragePeriodAndCycleLength(data, lastThreeUserPeriods);

			CalculateLastPeriodAndCycle(data, lastThreeUserPeriods);

			CalculatePercentages(data, lastThreeUserPeriods, userCycleDuration);

			AddPeriodHistory(data, lastThreeUserPeriods);

			response.Data = data;
			return response;
		}

		private static void CalculateNextPeriodStartDate(HomePageDataDto data, int userCycleDuration, PeriodHistory lastPeriod, out DateTime nextPeriod)
		{
			var nextPeriodStartDate = lastPeriod.StartDate;

			while (nextPeriodStartDate.AddDays(userCycleDuration) < DateTime.UtcNow.Date)
			{
				nextPeriodStartDate = nextPeriodStartDate.AddDays(userCycleDuration);
			}

			nextPeriod = nextPeriodStartDate.AddDays(userCycleDuration);
			data.NextPeriod = nextPeriod;
		}

		private static void CalculateNextOvulationAndFertileDay(HomePageDataDto data, DateTime nextPeriod, int userCycleDuration)
		{
			var potentialOvulationDate = data.NextPeriod?.AddDays(-14);

			if (potentialOvulationDate < DateTime.UtcNow.Date)
			{
				var nextOvulation = nextPeriod.AddDays(userCycleDuration).AddDays(-14);
				data.NextOvulation = nextOvulation;
				data.NextFertileDay = nextOvulation.AddDays(-5);
			}
			else
			{
				data.NextOvulation = potentialOvulationDate;
				var potentialFertileDate = potentialOvulationDate?.AddDays(-5);

				if (potentialFertileDate < DateTime.UtcNow.Date)
				{
					data.NextFertileDay = nextPeriod.AddDays(userCycleDuration).AddDays(-14).AddDays(-5);
				}
				else
				{
					data.NextFertileDay = potentialFertileDate;
				}
			}
		}

		private static void CalculateAveragePeriodAndCycleLength(HomePageDataDto data, List<PeriodHistory> lastThreeUserPeriods)
		{
			var totalPeriodDuration = 0;
			var totalCycleLength = 0;
			var minCycleLength = int.MaxValue;
			var maxCycleLength = int.MinValue;

			for (int i = 0; i < lastThreeUserPeriods.Count; i++)
			{
				var period = lastThreeUserPeriods[i];
				totalPeriodDuration += (period.EndDate - period.StartDate).Days + 1;

				if (i > 0)
				{
					var cycleLength = (lastThreeUserPeriods[i - 1].StartDate - period.StartDate).Days;

					totalCycleLength += cycleLength;

					minCycleLength = cycleLength < minCycleLength ? cycleLength : minCycleLength;
					maxCycleLength = cycleLength > maxCycleLength ? cycleLength : maxCycleLength;
				}
			}

			if (lastThreeUserPeriods.Count > 0)
			{
				data.AveragePeriodLength!.Value = totalPeriodDuration / lastThreeUserPeriods.Count;
				data.AveragePeriodLength.Status = CalculatePeriodLengthStatus((double)data.AveragePeriodLength.Value);
			}
			else
			{
				data.AveragePeriodLength = null;
			}

			if (lastThreeUserPeriods.Count > 1)
			{
				data.AverageCycleLength!.Value = totalCycleLength / (lastThreeUserPeriods.Count - 1);
				data.AverageCycleLength.Status = CalculateCycleLengthStatus((double)data.AverageCycleLength.Value);

				var variationStatusFirst = CalculateCycleLengthStatus(minCycleLength);
				var variationStatusSecond = CalculateCycleLengthStatus(maxCycleLength);

				data.CycleLengthVariation!.Value = Convert.ToString($"{minCycleLength} - {maxCycleLength}");

				if (variationStatusFirst == Status.Abnormal || variationStatusSecond == Status.Abnormal)
				{
					data.CycleLengthVariation.Status = Status.Abnormal;
				}
				else if (variationStatusFirst == Status.Warning || variationStatusSecond == Status.Warning)
				{
					data.CycleLengthVariation.Status = Status.Warning;
				}
				else
				{
					data.CycleLengthVariation.Status = Status.Normal;
				}
			}
			else
			{
				data.AverageCycleLength = null;
			}
		}

		private static void CalculateLastPeriodAndCycle(HomePageDataDto data, List<PeriodHistory> lastThreeUserPeriods)
		{
			if (lastThreeUserPeriods == null || lastThreeUserPeriods.Count == 0)
			{
				throw new ArgumentException("Lista menstruacija ne može biti prazna.", nameof(lastThreeUserPeriods));
			}

			lastThreeUserPeriods = lastThreeUserPeriods.OrderBy(p => p.StartDate).ToList();
			var lastPeriod = lastThreeUserPeriods.Last();
			int lastPeriodDuration = (lastPeriod.EndDate - lastPeriod.StartDate).Days + 1;

			data.PreviousPeriodLength!.Value = lastPeriodDuration;
			data.PreviousPeriodLength.Status = CalculatePeriodLengthStatus(lastPeriodDuration);

			if (lastThreeUserPeriods.Count > 1)
			{
				var lastIndex = lastThreeUserPeriods.Count - 1;
				data.PreviousCycleLength!.Value = (lastThreeUserPeriods[lastIndex].StartDate - lastThreeUserPeriods[lastIndex - 1].StartDate).Days;
				data.PreviousCycleLength.Status = CalculateCycleLengthStatus((double)data.PreviousCycleLength.Value);
			}
		}

		private static void CalculatePercentages(HomePageDataDto data, List<PeriodHistory> allUserPeriods, int userCycleDuration)
		{
			allUserPeriods.Reverse();

			if (allUserPeriods.Count > 1)
			{
				var onTimeCount = 0;
				var earlyCount = 0;
				var lateCount = 0;

				for (int i = 1; i < allUserPeriods.Count; i++)
				{
					var previousPeriod = allUserPeriods[i - 1];
					var currentPeriod = allUserPeriods[i];

					var expectedStartDate = previousPeriod.StartDate.AddDays(userCycleDuration);

					if (currentPeriod.StartDate == expectedStartDate)
					{
						onTimeCount++;
					}
					else if (currentPeriod.StartDate < expectedStartDate)
					{
						earlyCount++;
					}
					else if (currentPeriod.StartDate > expectedStartDate)
					{
						lateCount++;
					}
				}

				var totalPeriods = allUserPeriods.Count - 1;
				data.PercentageOfOnTimePeriods = (int)((onTimeCount / (double)totalPeriods) * 100);
				data.PercentageOfEarlyPeriods = (int)((earlyCount / (double)totalPeriods) * 100);
				data.PercentageOfLatePeriods = (int)((lateCount / (double)totalPeriods) * 100);
			}
			else
			{
				data.PercentageOfOnTimePeriods = null;
				data.PercentageOfEarlyPeriods = null;
				data.PercentageOfLatePeriods = null;
			}
		}

		private static void AddPeriodHistory(HomePageDataDto data, List<PeriodHistory> allUserPeriods)
		{
			var pastPeriods = new List<PastPeriodDto>();

			foreach (var period in allUserPeriods)
			{
				pastPeriods.Add(new PastPeriodDto
				{
					FromTo = $"{period.StartDate.ToString("dd.MM")} - {period.EndDate.ToString("dd.MM")}",
					PeriodDuration = (period.EndDate - period.StartDate).Days + 1,
				});
			}

			pastPeriods.Reverse();
			data.PastPeriods = pastPeriods;
		}

		private static Status CalculatePeriodLengthStatus(double periodDuration)
		{
			if (periodDuration < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(periodDuration), "Menstruacija ne može trajati manje od 0 dana.");
			}

			const double normalMinDuration = 3;
			const double normalMaxDuration = 8;
			const double warningDurationThreshold = 10;

			if (periodDuration >= normalMinDuration && periodDuration <= normalMaxDuration)
			{
				return Status.Normal;
			}
			else if ((periodDuration > normalMaxDuration && periodDuration <= warningDurationThreshold) || (periodDuration >= 0 && periodDuration < normalMinDuration))
			{
				return Status.Warning;
			}
			else
			{
				return Status.Abnormal;
			}
		}

		private static Status CalculateCycleLengthStatus(double cycleLength)
		{
			const double normalMinCycleLength = 21;
			const double normalMaxCycleLength = 35;
			const double warningCycleLengthThreshold = 45;
			const double abnormalMinCycleThreshold = 16;

			if (cycleLength < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(cycleLength), "Cycle length cannot be negative.");
			}

			if (cycleLength >= normalMinCycleLength && cycleLength <= normalMaxCycleLength)
			{
				return Status.Normal;
			}
			else if ((cycleLength > normalMaxCycleLength && cycleLength <= warningCycleLengthThreshold) ||
					 (cycleLength < normalMinCycleLength && cycleLength >= abnormalMinCycleThreshold))
			{
				return Status.Warning;
			}
			else
			{
				return Status.Abnormal;
			}
		}
	}
}
