using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Api;
using NomenclatureServer.Services;

namespace NomenclatureServer.Registration;

[ApiController]
[Route("[controller]")]
public class RegistrationController(LodestoneService lodestoneService, RegisteredNamesService registeredNames, ILogger<RegistrationController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] RegisterCharacterInitiateRequest request)
    {
        var result = await lodestoneService.Initiate(request.CharacterName, request.WorldName);
        return Ok(result);
    }
    
    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] RegisterCharacterValidateRequest request)
    {
        var result = await lodestoneService.Validate(request.CharacterName, request.ValidationCode);
        if(result is LodestoneErrorCode.Success)
        {
            string? res = await registeredNames.RegisterName(request.CharacterName);
            if (res is not null)
            {
                return Ok(res);
            }
        }
        logger.LogError(result.ToString());
        return BadRequest(request.CharacterName);
    }
}