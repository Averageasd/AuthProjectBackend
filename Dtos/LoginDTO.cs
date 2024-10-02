using System.ComponentModel.DataAnnotations;

namespace AuthProject.Dtos
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "User name cannot be empty")]
        public string? Name { get; set; }
        
        [Required(ErrorMessage = "Password cannot be empty")]
        public string? Password { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
