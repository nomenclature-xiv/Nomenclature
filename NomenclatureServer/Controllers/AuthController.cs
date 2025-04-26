using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NomenclatureCommon.Domain.Api.Controller;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;

namespace NomenclatureServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration config, DatabaseService db) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] GenerateTokenRequest request)
    {
        // DB Request
        if (await db.GetRegisteredCharacter(request.Secret) is not { } registeredCharacter)
            return Unauthorized("There are no registered characters with this secret.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SigningKey));
        var claims = new List<Claim>
        {
            new(AuthClaimType.CharacterName, registeredCharacter.Name),
            new(AuthClaimType.WorldName, registeredCharacter.World)
        };
        
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };

        return Ok(new JwtSecurityTokenHandler().CreateJwtSecurityToken(descriptor).RawData);
    }
}