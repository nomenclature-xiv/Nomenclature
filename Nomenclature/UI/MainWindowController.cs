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
        public MainWindowController(Configuration configuration)
        {
            ChangedName = configuration.Name;
        }

        public string BlocklistName = string.Empty;
        public int BlocklistWorld = 0;
    }
}
