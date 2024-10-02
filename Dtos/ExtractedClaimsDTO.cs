using System.Security.Claims;

namespace AuthProject.Dtos
{
    public class ExtractedClaimsDTO
    {
        public string? Name { get; set; }
        public ClaimsPrincipal? Principal { get; set; }
    }
}
