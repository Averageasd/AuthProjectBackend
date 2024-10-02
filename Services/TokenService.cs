using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthProject.Services
{
    public class TokenService : ITokenService
    {
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345superSecretKey@345"));

            // encode token with hmacsha256
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);


            var tokenOptions = new JwtSecurityToken
            (
                issuer: "https://localhost:7226",
                audience: "https://localhost:7226",
                claims: claims,

                // 5 minutes till token expires
                expires: DateTime.Now.AddMinutes(5),

                signingCredentials: signinCredentials
            );


            // string representation of jwt token
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return tokenString; 
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        // extract user from expired token
        // extracts the claims presented by a user in an HTTP request
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidParamters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345superSecretKey@345")),

                // dont care about token's expiration date.
                // because this is an expired token
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;


            // token: expired token
            // tokenValidParamters: parameters used to valdiate token
            // securityToken: validated token
            var principal = tokenHandler.ValidateToken(token, tokenValidParamters, out securityToken);

            // convert token to jwtSecurityToken
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            
            // if security token is null or the algorithm used to encode it is false
            // then we throw an exception
            if (jwtSecurityToken == null 
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
