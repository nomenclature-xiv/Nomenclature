using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureCommon.Domain.Api.Types
{
    [Flags] public enum ChangeItems
    {
        Name = 1,
        World = 2
    }
}
