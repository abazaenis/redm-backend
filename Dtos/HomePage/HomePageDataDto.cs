namespace Redm_backend.Dtos.HomePage
{
	using Redm_backend.Dtos.PeriodHistory;

	public class HomePageDataDto
	{
		public Dictionary<string, GetPeriodDto>? PeriodData { get; set; }

		public DateTime? NextPeriod { get; set; }

		public DateTime? NextOvulation { get; set; }

		public DateTime? NextFertileDay { get; set; }

		public ValueStatusPairDto<int?>? PreviousPeriodLength { get; set; } = new ValueStatusPairDto<int?>();

		public ValueStatusPairDto<int?>? PreviousCycleLength { get; set; } = new ValueStatusPairDto<int?>();

		public ValueStatusPairDto<double?>? AveragePeriodLength { get; set; } = new ValueStatusPairDto<double?>();

		public ValueStatusPairDto<double?>? AverageCycleLength { get; set; } = new ValueStatusPairDto<double?>();

		public ValueStatusPairDto<string?>? CycleLengthVariation { get; set; } = new ValueStatusPairDto<string?>();

		public int? PercentageOfOnTimePeriods { get; set; }

		public int? PercentageOfEarlyPeriods { get; set; }

		public int? PercentageOfLatePeriods { get; set; }

		public List<PastPeriodDto>? PastPeriods { get; set; }
	}
}
