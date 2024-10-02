using AuthProject.Context;
using AuthProject.Dtos;
using AuthProject.Models;
using AuthProject.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace AuthProject.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DapperContext _dapperContext;
        private readonly ITokenService _tokenService;
        public AuthController(DapperContext dapperContext, ITokenService tokenService) {
            _dapperContext = dapperContext; 
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    string.Join("\n", ModelState.Values
                        .SelectMany(
                            err => err.Errors
                        )
                        .Select(err => err.ErrorMessage))
                    );
            }

            var query = "SELECT * FROM AuthUser WHERE Name = @Name AND Password = @Password";
            var updateQuery = "UPDATE AuthUser SET RefreshToken = @refreshToken, RefreshTokenExpiryTime = @refreshTokenExpiryTime WHERE Name = @Name AND Password = @Password";
            AuthUser? authUser = null;
            string accessToken = null;
            string refreshToken = null;
            using (IDbConnection connection = _dapperContext.CreateConnection())
            {
                try
                {
                    authUser = await connection.QuerySingleOrDefaultAsync<AuthUser>(
                    query,
                    new
                    {
                        loginDTO.Name,
                        loginDTO.Password
                    });
                    if (authUser == null)
                    {
                        return Unauthorized();
                    }

                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginDTO.Name),
                    new Claim(ClaimTypes.Role, "Manager")
                };

                    // access token to have access to protected routes
                    accessToken = _tokenService.GenerateAccessToken(claims);

                    // refresh token used to generate access token
                    refreshToken = _tokenService.GenerateRefreshToken();

                    // refresh token expiry time
                    var refeshTokenExpiryTime = DateTime.Now.AddDays(7);

                    var dynamicParamters = new DynamicParameters();
                    dynamicParamters.Add("Name", authUser.Name);
                    dynamicParamters.Add("Password", authUser.Password);
                    dynamicParamters.Add("RefreshToken", refreshToken);
                    dynamicParamters.Add("RefreshTokenExpiryTime", refeshTokenExpiryTime);
                    await connection.ExecuteAsync(updateQuery, dynamicParamters);
                }
                catch (Exception ex) {
                    return BadRequest();
                }

            };

            return Ok(new AuthenticatedResponse()
            {
                Token = accessToken,
                RefreshToken = refreshToken,
            });
        }
    }
}
