namespace Redm_backend.Dtos.Symptom
{
    public class SymptomDto
    {
        public DateTime Date { get; set; }

        public GroupedSymptomsDto PhysicalSymptoms { get; set; } = new GroupedSymptomsDto();

        public GroupedSymptomsDto MoodSymptoms { get; set; } = new GroupedSymptomsDto();

        public GroupedSymptomsDto SexualActivitySymptoms { get; set; } = new GroupedSymptomsDto();

        public GroupedSymptomsDto OtherSymptoms { get; set; } = new GroupedSymptomsDto();
    }
}