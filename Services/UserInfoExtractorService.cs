using AuthProject.Dtos;
using AuthProject.Models;

namespace AuthProject.Services
{
    public class UserInfoExtractorService
    {

        public static ExtractedClaimsDTO GetExtractedInfo(
            ITokenService tokenService,
            TokenApiModel tokenApiModel
        )
        {
            string accessToken = tokenApiModel.AccessToken!;

            var principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userName = principal.Identity.Name;

            return new ExtractedClaimsDTO()
            {
                Name = userName,
                Principal = principal,
            };
        }
    }
}
