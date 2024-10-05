namespace Redm_backend.Services.HomePageService
{
	using System.Security.Claims;

	using Microsoft.EntityFrameworkCore;

	using Redm_backend.Data;
	using Redm_backend.Dtos.HomePage;
	using Redm_backend.Models;
	using Redm_backend.Services.PeriodService;
	using Redm_backend.Services.UserService;

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
			var userPeriodDuration = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue("PeriodDuration")!);
			var userCycleDuration = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue("CycleDuration")!);
			var allUserPeriods = await _context.PeriodHistory.Where(ph => ph.UserId == userId)
													 .OrderByDescending(ph => ph.StartDate)
													 .ToListAsync();
			var lastPeriod = allUserPeriods.FirstOrDefault();

			// Starting to fill DTO

			// Period Data
			data.PeriodData = (await _periodHistoryService.GetPeriodsAndPredictions()).Data;

			// Next period
			if (lastPeriod != null)
			{
				var nextPeriodStartDate = lastPeriod.StartDate;

				while (nextPeriodStartDate.AddDays(userCycleDuration) < DateTime.UtcNow.Date)
				{
					nextPeriodStartDate = nextPeriodStartDate.AddDays(userCycleDuration);
				}

				var nextPeriod = nextPeriodStartDate.AddDays(userCycleDuration);
				data.NextPeriod = nextPeriod;

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

				var totalPeriodDuration = 0;
				var totalCycleLength = 0;

				for (int i = 0; i < allUserPeriods.Count; i++)
				{
					var period = allUserPeriods[i];
					totalPeriodDuration += (period.EndDate - period.StartDate).Days + 1;

					if (i > 0)
					{
						totalCycleLength += (allUserPeriods[i - 1].StartDate - period.StartDate).Days;
					}
				}

				data.AveragePeriodDuration = totalPeriodDuration / allUserPeriods.Count;

				if (allUserPeriods.Count > 1)
				{
					data.AverageCycleDuration = totalCycleLength / (allUserPeriods.Count - 1);
				}
				else
				{
					data.AverageCycleDuration = null;
				}

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

				var pastPeriods = new List<PastPeriodDto>();

				foreach (var period in allUserPeriods)
				{
					pastPeriods.Add(new PastPeriodDto
					{
						FromTo = $"{period.StartDate.ToString("dd.MM")} - {period.EndDate.ToString("dd.MM")}",
						PeriodDuration = (period.EndDate - period.StartDate).Days + 1,
					});
				}

				data.PastPeriods = pastPeriods;
			}
			else
			{
				data.NextPeriod = null;
				data.NextOvulation = null;
				data.NextFertileDay = null;
			}

			response.Data = data;
			return response;
		}
	}
}
