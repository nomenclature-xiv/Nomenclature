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
    public string RegistrationCode = string.Empty;
    public bool ShowLodestoneRegistrationText = false;

    public async void StartRegistrationXivAuth()
    {
        if (await framework.RunOnFrameworkThread(() => playerState.IsLoaded is false))
            return;
        try
        {
            Character self = await framework.RunOnFrameworkThread(() => { return new Character(playerState.CharacterName, playerState.HomeWorld.Value.Name.ToString()); });
            try
            {
                var res = await registerService.RegisterCharacterXivAuth(self);
                if (res is null)
                {
                    ErrorMessage = "Timed out! Please try again.";
                    return;
                }
                await configuration.InitSecretAsync(self.Name, self.World, res);
                //updates character registration status
                configuration.CheckAnyCharacterRegistered();
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
    
    public async void StartRegistrationLodestone()
    {
        if (await framework.RunOnFrameworkThread(() => playerState.IsLoaded is false))
            return;
        try
        {
            Character self = await framework.RunOnFrameworkThread(() => { return new Character(playerState.CharacterName, playerState.HomeWorld.Value.Name.ToString()); });
            try
            {
                var res = await registerService.RegisterCharacterLodestone(self);
                if (res is null)
                {
                    ErrorMessage = "Could not reach registration server. Please try again, or check server status.";
                    return;
                }
                RegistrationCode = res;
                ShowLodestoneRegistrationText = true;
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

    public async void CheckRegistrationLodestone()
    {
        try
        {
            if (await framework.RunOnFrameworkThread(() => playerState.IsLoaded is false))
                return;

            if (await registerService.RegisterCharacterValidateLodestone(RegistrationCode) is not { } secret)
            {
                ErrorMessage = "Did not find code in character bio!";
                return;
            }

            var character = new Character(playerState.CharacterName, playerState.HomeWorld.Value.Name.ToString());
            var config = new CharacterConfiguration(secret);
            await configuration.InitSecretAsync(character.Name, character.World, secret);
            //updates character registration status
            configuration.CheckAnyCharacterRegistered();
            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}