namespace Redm_backend.Dtos.Symptom
{
    public class GetSymptomDto
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public GroupedSymptomsDto PhysicalSymptoms { get; set; } = new GroupedSymptomsDto();

        public GroupedSymptomsDto MoodSymptoms { get; set; } = new GroupedSymptomsDto();

        public GroupedSymptomsDto SexualActivitySymptoms { get; set; } = new GroupedSymptomsDto();

        public GroupedSymptomsDto OtherSymptoms { get; set; } = new GroupedSymptomsDto();
    }
}