using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Redm_backend.Dtos.Auth
{
    public class GoogleSignInVMDto
    {
        [Required]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [Required]
        [EmailAddress] // This will validate the email format
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required]
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [Required]
        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
