using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NomenclatureCommon.Domain.Api.Login;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;

namespace NomenclatureServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration config, DatabaseService database, ILogger<AuthController> logger) : ControllerBase
{
    // Const
    private static readonly Version ExpectedVersion = new(0, 5, 0, 0);

    // Instantiated
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config.SigningKey));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginAuthenticationRequest request)
    {
        if (request.Version < ExpectedVersion)
            return StatusCode(StatusCodes.Status409Conflict, new LoginAuthenticationResponse(LoginAuthenticationErrorCode.VersionMismatch));

        if (await database.GetRegisteredCharacter(request.Secret) is not { } registeredCharacter)
            return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResponse(LoginAuthenticationErrorCode.UnknownSecret));

        var token = GenerateJwtToken([new Claim(AuthClaimType.CharacterIdentifier, registeredCharacter)]);
        return StatusCode(StatusCodes.Status200OK, new LoginAuthenticationResponse(LoginAuthenticationErrorCode.Success, token.RawData));
    }

    private JwtSecurityToken GenerateJwtToken(List<Claim> claims)
    {
        var token = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };

        return new JwtSecurityTokenHandler().CreateJwtSecurityToken(token);
    }
}