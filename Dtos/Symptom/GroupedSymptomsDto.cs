namespace Redm_backend.Dtos.Symptom
{
    public class GroupedSymptomsDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Symptoms { get; set; } = new List<string>();
    }
}