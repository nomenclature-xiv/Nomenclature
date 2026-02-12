using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Exceptions;
using System;

namespace NomenclatureClient.UI.Views.Register;

public class RegisterViewController(IFramework framework, IPlayerState playerState, ConfigurationService configuration, NetworkRegisterService registerService)
{
    public string SecretIdentifier = string.Empty;
    public string SecretKey = string.Empty;
    public string ErrorMessage = string.Empty;

    public async void StartRegistration()
    {
        if (await framework.RunOnFrameworkThread(() => playerState.IsLoaded is false))
            return;
        try
        {
            Character self = await framework.RunOnFrameworkThread(() => { return new Character(playerState.CharacterName, playerState.HomeWorld.Value.Name.ToString()); });
            try
            {
                var res = await registerService.RegisterCharacter(self);
                if (res is null)
                {
                    ErrorMessage = "Timed out! Please try again.";
                    return;
                }
                await configuration.InitSecretAsync(self.Name, self.World, res);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        catch (CharacterNotMatchingException ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}