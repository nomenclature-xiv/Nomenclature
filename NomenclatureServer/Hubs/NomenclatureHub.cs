using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Api;
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

    [HubMethodName(ApiMethods.ClearName)]
    public GenericResponse ClearName(ClearNameRequest request)
    {
        logger.LogInformation("{Request}", request);
        registeredNamesService.ActiveNameChanges.Remove(RegisteredCharacter);
        return new GenericResponse();
    }
    
    [HubMethodName(ApiMethods.SetName)]
    public GenericResponse RegisterName(NewNameRequest request)
    {
        logger.LogInformation("{Request}", request);
        registeredNamesService.ActiveNameChanges[RegisteredCharacter] = request.Name;
        return new GenericResponse() { Success = true };
    }

    [HubMethodName(ApiMethods.QueryChangedNames)]
    public QueryChangedNamesResponse QueryChangedNames(QueryChangedNamesRequest request)
    {
        logger.LogInformation("{Request}", request);
        
        var span = request.NamesToQuery.AsSpan();
        var output = new Dictionary<string, string>();
        
        for (var i = 0; i < span.Length; i++)
            if (registeredNamesService.ActiveNameChanges.TryGetValue(span[i], out var name))
                output.Add(span[i], name);

        return new QueryChangedNamesResponse { ModifiedNames = output };
    }
}