using AuthProject.Context;
using AuthProject.Dtos;
using AuthProject.Models;
using AuthProject.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AuthProject.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly DapperContext _dapperContext;
        private readonly ITokenService _tokenService;

        public TokenController(DapperContext dapperContext, ITokenService tokenService)
        {
            _dapperContext = dapperContext;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody]TokenApiModel tokenApiModel)
        {
            if (tokenApiModel == null)
            {
                return BadRequest("Invalid client request");
            }

            string accessToken = tokenApiModel.AccessToken!;
            string refreshToken = tokenApiModel.RefreshToken!;

            //var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            //var userName = principal.Identity.Name;

            string? newAccessToken = null;
            string? newRefreshToken = null;

            ExtractedClaimsDTO extractedClaimsDTO = UserInfoExtractorService.GetExtractedInfo(_tokenService, tokenApiModel);

            var singleUserWithMatchingNameQuery = "SELECT * FROM AuthUser WHERE Name = @userName";
            var updateRefreshTokenForUser = "UPDATE AuthUser SET RefreshToken = @newRefreshToken WHERE Id = @Id";

            using (IDbConnection connection = _dapperContext.CreateConnection())
            {
                try
                {
                    var user = await connection.QuerySingleOrDefaultAsync<AuthUser>(
                    singleUserWithMatchingNameQuery,
                    new
                    {
                        userName = extractedClaimsDTO.Name
                    }
                );


                    // user cannot be found
                    // refresh token value does not match the one in database
                    // refresh token expires (ex. token expires 7th day from creation. Today is 8th day so it expires)
                    if (
                        user == null
                        || user.RefreshToken != refreshToken
                        || user.RefreshTokenExpiryTime <= DateTime.Now
                    )
                    {
                        return BadRequest("Invalid client request");
                    }

                    // generate new access token and refresh token
                    newAccessToken = _tokenService.GenerateAccessToken(extractedClaimsDTO.Principal!.Claims);
                    newRefreshToken = _tokenService.GenerateRefreshToken();

                    // save new refresh token in database
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("Id", user.Id);
                    dynamicParameters.Add("newRefreshToken", newRefreshToken);
                    await connection.ExecuteAsync(updateRefreshTokenForUser, dynamicParameters);
                }
                catch (Exception ex)
                {
                    return BadRequest();
                }
            }

            return Ok(new AuthenticatedResponse()
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [Authorize]
        [HttpPost]
        [Route("revoke")]
        public async Task<IActionResult> Revoke(TokenApiModel tokenApiModel)
        {
            if (tokenApiModel == null)
            {
                return BadRequest("Invalid client request");
            }

            ExtractedClaimsDTO extractedClaimsDTO = UserInfoExtractorService.GetExtractedInfo(_tokenService, tokenApiModel);
            string queryUserWithUserName = "SELECT * FROM AuthUser WHERE Name = @Name";
            string revokeTokenQuery = "UPDATE AuthUser SET RefreshToken = NULL WHERE Id = @Id";

            using (IDbConnection connection = _dapperContext.CreateConnection())
            {
                try
                {
                    var user = await connection.QuerySingleOrDefaultAsync<AuthUser>(queryUserWithUserName, new
                    {
                        extractedClaimsDTO.Name
                    });
                    if (user == null) return BadRequest();
                    await connection.ExecuteAsync(revokeTokenQuery, new
                    {
                        user.Id
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest();
                }
            }
            return NoContent();
        }
    }
}
