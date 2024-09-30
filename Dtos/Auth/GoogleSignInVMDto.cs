using System.ComponentModel.DataAnnotations;

namespace Redm_backend.Dtos.Auth
{
    public class GoogleSignInVMDto
    {
        [Required]
        public string IdToken { get; set; }
    }
}