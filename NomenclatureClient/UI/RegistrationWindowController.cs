using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.UI;

public class RegistrationWindowController(
    Configuration configuration,
    IClientState clientState,
    SessionService sessionService,
    NetworkRegisterService networkRegisterService)
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
    ///     Attempts to register the local character for use with Nomenclature
    /// </summary>
    public async void BeginRegistration()
    {
        try
        {
            if (clientState.LocalPlayer is not { } player)
                return;

            var character = new Character(player.Name.ToString(), player.HomeWorld.Value.Name.ToString());
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
    public async Task<bool> ValidateRegistration()
    {
        try
        {
            ValidateRegistrationError = true;
            if (clientState.LocalPlayer is not { } player)
                return false;

            if (RegistrationKey is null)
                return false;

            if (await networkRegisterService.RegisterCharacterValidate(RegistrationKey) is not { } secret)
                return false;

            var character = new Character(player.Name.ToString(), player.HomeWorld.Value.Name.ToString());
            var config = new CharacterConfiguration(secret);
            sessionService.CurrentSession = new SessionInfo(character, config);
            configuration.LocalConfigurations.TryAdd(character.ToString(), config);
            configuration.Save();

            ValidateRegistrationError = false;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}