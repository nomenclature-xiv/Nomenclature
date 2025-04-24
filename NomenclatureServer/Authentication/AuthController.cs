using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NomenclatureCommon.Domain.Api;
using NomenclatureServer.Services;

namespace NomenclatureServer.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration configuration, DatabaseService databaseService, ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] TokenRequest request)
    {
        // DB Request
        if (await databaseService.GetRegisteredCharacter(request.Secret) is not { } registeredName)
        {
            logger.LogInformation("Secret {Secret} not found in database", request.Secret);
            return Unauthorized("There are no registered characters with this secret.");
        }
            
        
        var key = configuration.SigningKey;
        var claims = new List<Claim> { new(AuthClaimType.RegisteredCharacter, registeredName) };
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };
        
        return Ok(new JwtSecurityTokenHandler().CreateJwtSecurityToken(descriptor).RawData);
    }
}