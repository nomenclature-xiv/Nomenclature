using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain.Api.Controller;
using NomenclatureServer.Services;

namespace NomenclatureServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController(DatabaseService database, OauthService authService, ILogger<RegistrationController> logger) : ControllerBase
{
    /// <summary>
    ///     List of all current registrations
    /// </summary>
    private static readonly Dictionary<string, string> PendingRegistrations = new();

    [AllowAnonymous]
    [HttpPost("initiate")]
    public IActionResult Initiate([FromBody] BeginCharacterRegistrationRequest request)
    {
        string ticket = authService.GetSetTicket();
        string url = authService.GetAuthorizationUrl(ticket);
        var res = new BeginCharacterRegistrationResponse(true, ticket, url);
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCharacterRegistration request)
    {
        try
        {
            var token = await authService.ValidateTicket(request.Character, request.Ticket);
            if (token is null)
                return Ok(new ValidateCharacterRegistrationResponse() { Status = "unbound", Token = null });
            return Ok(new ValidateCharacterRegistrationResponse() { Status = "bound", Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task Callback([FromQuery] string code, [FromQuery] string state)
    {
        var request = HttpContext.Request;
        var res = await authService.GetBearerToken(code, $"{request.Scheme}://{request.Host}{request.Path}");
        if(res is null)
        {
            return;
        }
        await authService.GetCharacter(res, state);
    }
}