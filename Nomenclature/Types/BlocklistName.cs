using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nomenclature.Types
{
    public class BlocklistCharacter
    {
        public Guid GUID = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string World { get; set; } = string.Empty;
    }
}
