namespace Redm_backend.Dtos.HomePage
{
    using Redm_backend.Dtos.PeriodHistory;

    public class HomePageDataDto
	{
		public Dictionary<string, GetPeriodDto>? PeriodData { get; set; }

		public DateTime? NextPeriod { get; set; }

		public DateTime? NextOvulation { get; set; }

		public DateTime? NextFertileDay { get; set; }

		public double? AveragePeriodDuration { get; set; }

		public double? AverageCycleDuration { get; set; }

		public int? PercentageOfOnTimePeriods { get; set; }

		public int? PercentageOfEarlyPeriods { get; set; }

		public int? PercentageOfLatePeriods { get; set; }

		// TODO: Ima još nešto, pitaj ibru
    }
}
