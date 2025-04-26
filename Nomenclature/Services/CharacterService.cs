using Dalamud.Plugin.Services;
using NomenclatureCommon.Domain;
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

        public async Task<Character?> GetCurrentCharacter()
        {
            return await _frameworkService.RunOnFramework(GetCurrentCharacterOnThread);
        }

        private Character? GetCurrentCharacterOnThread()
        {
            var local = _clientState.LocalPlayer;
            if (local is null)
            {
                return null;
            }
            return new Character(local.Name.TextValue, local.HomeWorld.Value.Name.ExtractText());
        }
    }
}
