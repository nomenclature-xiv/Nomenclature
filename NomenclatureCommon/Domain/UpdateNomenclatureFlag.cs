using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureCommon.Domain;

public enum UpdateNomenclatureFlag
{
    None = 0,
    Name = 1 << 0,
    World = 1 << 1
}
