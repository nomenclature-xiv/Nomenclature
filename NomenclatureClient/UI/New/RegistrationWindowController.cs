using System;
using Dalamud.Plugin.Services;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.New;

public class RegistrationWindowController(IPluginLog log, Configuration configuration, CharacterService characterService, NetworkRegisterService networkRegisterService, NetworkHubService networkHubService)
{
    /// <summary>
    ///     The registration key generated from <see cref="BeginRegistration"/>
    /// </summary>
    public string? RegistrationKey;

    /// <summary>
    ///     If there was an error retrieving the local player's lodestone details
    /// </summary>
    public bool BeginRegistrationError;
    
    /// <summary>
    ///     If there was an error validating the local player's code on their lodestone profile
    /// </summary>
    public bool ValidateRegistrationError;

    /// <summary>
    ///     If the current validation was successful
    /// </summary>
    public bool SuccessfulValidation;

    /// <summary>
    ///     Attempts to register the local character for use with Nomenclature
    /// </summary>
    public async void BeginRegistration()
    {
        try
        {
            if (characterService.CurrentCharacter is not { } character)
                return;

            if (await networkRegisterService.RegisterCharacterInitiate(character) is { } key)
            {
                RegistrationKey = key;
                BeginRegistrationError = false;
            }
            else
            {
                BeginRegistrationError = true;
            }
        }
        catch (Exception)
        {
            BeginRegistrationError = true;
        }
    }

    /// <summary>
    ///     Attempts to validate the local character's generated code from <see cref="BeginRegistration"/> on their lodestone profile
    /// </summary>
    public async void ValidateRegistration()
    {
        try
        {
            SuccessfulValidation = false;
            if (characterService.CurrentCharacter is not { } character)
                return;
            
            if (RegistrationKey is null)
                return;
            
            if (await networkRegisterService.RegisterCharacterValidate(RegistrationKey) is not { } secret)
            {
                ValidateRegistrationError = true;
                return;
            }
            
            configuration.LocalCharacterSecrets.Add(character.ToString(), secret);
            configuration.Save();

            SuccessfulValidation = true;
            ValidateRegistrationError = false;
            networkHubService.Connect().ConfigureAwait(false);
        }
        catch (Exception)
        {
            ValidateRegistrationError = true;
        }
    }
}