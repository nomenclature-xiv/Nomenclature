using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon;
using NomenclatureCommon.Domain.Api;
using NomenclatureServer.Authentication;
using NomenclatureServer.Services;

namespace NomenclatureServer.Hubs;

[Authorize]
public class NomenclatureHub(RegisteredNamesService registeredNamesService, ILogger<NomenclatureHub> logger) : Hub
{
    /// <summary>
    ///     Registered character obtained from authenticated jwt token claims
    /// </summary>
    private string RegisteredCharacter =>
        Context.User?.Claims.FirstOrDefault(claim =>
            string.Equals(claim.Type, AuthClaimType.RegisteredCharacter, StringComparison.Ordinal))?.Value ??
        throw new Exception("RegisteredCharacter is not present in provided claims");

    [HubMethodName(ApiMethod.RegisterName)]
    public RegisterNameResponse RegisterName(RegisterNameRequest request)
    {
        logger.LogInformation("{Request}", request);
        if (registeredNamesService.ActiveNameChanges.ContainsKey(RegisteredCharacter))
        {
            registeredNamesService.ActiveNameChanges[RegisteredCharacter] = request.Name;
        }
        else
        {
            registeredNamesService.ActiveNameChanges.Add(RegisteredCharacter, request.Name);
        }
        return new RegisterNameResponse
        {
            Success = true
        };
    }

    [HubMethodName(ApiMethod.QueryChangedNames)]
    public QueryChangedNamesResponse QueryChangedNames(QueryChangedNamesRequest request)
    {
        logger.LogInformation("{Request}", request);
        var span = request.NamesToQuery.ToArray();
        var output = new Dictionary<string, string>();
        for (var i = 0; i < request.NamesToQuery.Count; i++)
        {
            if (registeredNamesService.ActiveNameChanges.ContainsKey(span[i]))
            {
                output.Add(span[i], registeredNamesService.ActiveNameChanges[span[i]]);
            }
        }

        return new() { ModifiedNames = output };

    }
}