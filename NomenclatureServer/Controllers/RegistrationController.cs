using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain.Api.Controller;
using NomenclatureServer.Services;

namespace NomenclatureServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController(DatabaseService database, LodestoneService lodestoneService, ILogger<RegistrationController> logger) : ControllerBase
{
    /// <summary>
    ///     List of all current registrations
    /// </summary>
    private static readonly Dictionary<string, string> PendingRegistrations = new();

    [AllowAnonymous]
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] BeginCharacterRegistrationRequest request)
    {
        if (await lodestoneService.GetCharacterLodestoneIdAsync(request.Character) is not { } lodestoneId)
            return NotFound();

        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        PendingRegistrations[key] = lodestoneId;
        return Ok(key);
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCharacterRegistration request)
    {
        logger.LogInformation("{Request}", request);
        
        if (PendingRegistrations.TryGetValue(request.ValidationCode, out var lodestoneId) is false)
            return BadRequest();

        if (await lodestoneService.GetLodestoneCharacterByLodestoneId(lodestoneId) is not { } character)
            return NotFound();

        if (character.Bio.Equals(request.ValidationCode) is false)
            return Conflict();

        PendingRegistrations.Remove(request.ValidationCode);

        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        if (await database.RegisterCharacter(secret, character.CharacterName, character.WorldName))
            return Ok(secret);

        return StatusCode(500);
    }
}