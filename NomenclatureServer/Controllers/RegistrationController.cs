using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NomenclatureClient.Network;
using NomenclatureCommon.Domain.Api.Controller;
using NomenclatureCommon.Domain.Api.Controller.Lodestone;
using NomenclatureCommon.Domain.Api.Controller.Oauth;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;
using NomenclatureServer.Types;
using NomenclatureServer.Utilities;

namespace NomenclatureServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController(DatabaseService database, OauthService authService, LodestoneAuthService lodestoneAuthService, ILogger<RegistrationController> logger) : ControllerBase
{

    [AllowAnonymous]
    [HttpPost("initiate/xivauth")]
    public IActionResult InitiateXivAuth()
    {
            string ticket = authService.GetSetTicket();
            string url = authService.GetAuthorizationUrl(ticket);
            var res = new BeginCharacterRegistrationResponseOauth(true, ticket, url);
            return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("initiate/lodestone")]
    public async Task<IActionResult> InitiateLodestone([FromBody] BeginCharacterRegistrationRequest request)
    {
        var key = await lodestoneAuthService.InitLodestoneAuth(request.Character);
        if (key is null)
        {
            return NotFound();
        }

        var res = new BeginCharacterRegistrationResponseLodestone(true, key);
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("validate/xivauth")]
    public async Task<IActionResult> ValidateXivAuth([FromBody] ValidateCharacterRegistrationOauth request)
    {
        try
        {
            var lodestone = authService.ValidateTicket(request.Character, request.Ticket);
            if (lodestone is null)
                return Ok(new ValidateCharacterRegistrationResponse() { Status = "unbound", Secret = null });
            var secret = AuthUtilities.GenerateSecret();
            var friendcode = AuthUtilities.GenerateFriendCode();
            var succ = await database.CreateAccount(lodestone.lodestone_id, friendcode, secret);
            if (succ != DatabaseResultEc.Success)
                return Ok(new ValidateCharacterRegistrationResponse() { Status = "Could not create account! Was this character already used to register an account?", Secret = null});
            return Ok(new ValidateCharacterRegistrationResponse() { Status = "bound", Secret = secret });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost("validate/lodestone")]
    public async Task<IActionResult> ValidateLodestone([FromBody] ValidateCharacterRegistrationLodestone request)
    {
        var result = await lodestoneAuthService.ValidateLodestoneAuth(request.Code);
        switch (result.Status)
        {
            case LodestoneValidationStatus.NoPendingRegistration:
                return BadRequest();
            case LodestoneValidationStatus.CharacterNotFound:
                return NotFound();
            case LodestoneValidationStatus.CodeNotFound:
                return Conflict();
            case LodestoneValidationStatus.UnknownError:
                return Problem();
        }

        return Ok(result.Secret);
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task Callback([FromQuery] string code, [FromQuery] string state)
    {
        var request = HttpContext.Request;
        var res = await authService.GetBearerToken(code, $"{NetworkConstants.BaseUrl}{request.Path}");
        if(res is null)
        {
            return;
        }
        await authService.GetCharacter(res, state);
    }
}