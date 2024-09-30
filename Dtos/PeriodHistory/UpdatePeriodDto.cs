namespace Redm_backend.Dtos.PeriodHistory
{
    public class UpdatePeriodDto
    {
        public int PeriodId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}