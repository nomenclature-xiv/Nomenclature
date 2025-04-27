using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain.Api.Controller;
using NomenclatureServer.Services;

namespace NomenclatureServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController(DatabaseService database, RegistrationService registrationService, ILogger<RegistrationController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] BeginCharacterRegistrationRequest request)
    {
        var result = await registrationService.BeginRegistrationAsync(request.Character);
        return Ok(result);
    }
    
    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCharacterRegistration request)
    {
        var secret = await registrationService.ValidateRegistrationAsync(request.ValidationCode).ConfigureAwait(false);
        return secret is null ? NotFound() : Ok(secret);
    }
}