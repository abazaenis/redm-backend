namespace Redm_backend.Dtos.Story
{
    public class AddSingleStoryDto
    {
        public int PostId { get; set; }

        public string? Title { get; set; }

        public string? Image { get; set; }

        public string? BackgroundColor { get; set; }
    }
}