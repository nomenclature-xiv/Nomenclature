using NomenclatureClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NomenclatureClient.Services
{
    public class NameService(NetworkNameService _networkNameService, CharacterService _characterService, Configuration _configuration)
    {
        public async Task ChangeName()
        {
            var name = _characterService.CurrentConfig.UseName ? _characterService.CurrentConfig.Name : null;
            var world = _characterService.CurrentConfig.UseWorld ? _characterService.CurrentConfig.World : null;

            await _networkNameService.ClearName();
            if (name is null && world is null)
            {
                return;
            }
            if (await _networkNameService.UpdateName(name, world))
            {
                _configuration.Save();
            }
        }
    }
}
