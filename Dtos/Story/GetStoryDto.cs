namespace Redm_backend.Dtos.Story
{
    public class GetStoryDto
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public string? Title { get; set; }

        public string? Image { get; set; }

        public string? BackgroundColor { get; set; }
    }
}