using NomenclatureClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NomenclatureClient.UI
{
    public class MainWindowController
    {
        private readonly NetworkNameService _nameService;
        private readonly Configuration _config;

        public string ChangedName = string.Empty;
        public string ChangedWorld = string.Empty;
        public bool SelfChangeNameEnabled = true;
        public bool SelfChangeWorldEnabled = true;
        public bool Locked = false;
        public MainWindowController(Configuration configuration, NetworkNameService nameService)
        {
            ChangedName = configuration.Name;
            ChangedWorld = configuration.World;
            SelfChangeNameEnabled = configuration.SelfChangeName;
            SelfChangeWorldEnabled = configuration.SelfChangeWorld;

            _nameService = nameService;
            _config = configuration;
        }

        public string BlocklistName = string.Empty;
        public int BlocklistWorld = 0;

        public async void UpdateName()
        {
            string? changename = SelfChangeNameEnabled ? ChangedName : null;
            string? changeworld = SelfChangeWorldEnabled ? ChangedWorld : null;
            await _nameService.ClearName();
            bool res = await _nameService.UpdateName(changename, changeworld);
            if (!res)
                return;

            _config.Name = ChangedName;
            _config.World = ChangedWorld;
            _config.Save();
        }

        public void ClearName()
        {
            _nameService.ClearName();
        }
    }
}
