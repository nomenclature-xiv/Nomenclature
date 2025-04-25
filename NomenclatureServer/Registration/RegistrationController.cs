using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomenclatureCommon.Api;
using NomenclatureServer.Services;

namespace NomenclatureServer.Registration;

[ApiController]
[Route("[controller]")]
public class RegistrationController(LodestoneService lodestoneService) : ControllerBase
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
        return Ok(request.CharacterName);
    }
}