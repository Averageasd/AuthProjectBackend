using System.ComponentModel.DataAnnotations;

namespace AuthProject.Models
{
    public class AuthUser
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? RefreshToken { get; set; }

        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}")]
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
