using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTO
{
    public class AuthRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
