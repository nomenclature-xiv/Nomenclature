using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nomenclature.UI
{
    public class MainWindowController
    {
        public string ChangedName = string.Empty;
        public string ChangedWorld = string.Empty;
        public bool SelfChangeNameEnabled = true;
        public MainWindowController(Configuration configuration)
        {
            ChangedName = configuration.Name;
            ChangedWorld = configuration.World;
            SelfChangeNameEnabled = configuration.SelfChangeName;
        }

        public string BlocklistName = string.Empty;
        public int BlocklistWorld = 0;
    }
}
