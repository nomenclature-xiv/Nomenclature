using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nomenclature.Services
{
    public class CharacterService
    {
        private readonly FrameworkService _frameworkService;
        private readonly IClientState _clientState;

        public CharacterService(FrameworkService frameworkService, IClientState clientState)
        {
            _clientState = clientState;
            _frameworkService = frameworkService;
        }

        public async Task<(string CharacterName, string WorldName)?> GetCurrentCharacter()
        {
            return await _frameworkService.RunOnFramework(GetCurrentCharacterOnThread);
        }

        private (string CharacterName, string WorldName)? GetCurrentCharacterOnThread()
        {
            var local = _clientState.LocalPlayer;
            if (local is null)
            {
                return null;
            }
            return (local.Name.TextValue, local.HomeWorld.Value.Name.ExtractText());
        }
    }
}
