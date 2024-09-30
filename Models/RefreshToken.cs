namespace Redm_backend.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string? Token { get; set; }

        public DateTime Expiration { get; set; }

        public User? User { get; set; }

        public int UserId { get; set; }
    }
}