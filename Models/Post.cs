namespace Redm_backend.Models
{
    public class Post
    {
        public int Id { get; set; }

        public int PostCategoryId { get; set; }

        public PostCategory? PostCategory { get; set; }

        public string? Title { get; set; }

        public string? Image { get; set; }

        public List<Story> Stories { get; set; } = new List<Story>();
    }
}