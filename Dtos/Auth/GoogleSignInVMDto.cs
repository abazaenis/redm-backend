using System.ComponentModel.DataAnnotations;

namespace Redm_backend.Dtos.Auth
{
    public class GoogleSignInVMDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [EmailAddress] // This will validate the email format
        public string Email { get; set; }

        [Required]
        public string GivenName { get; set; }

        [Required]
        public string FamilyName { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
