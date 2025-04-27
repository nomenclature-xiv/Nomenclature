using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NomenclatureCommon.Domain
{
    [MessagePackObject]
    public record CharacterIdentity(
        [property: Key(0)] Character Character,
        [property: Key(1)] Nomenclature Nomenclature)
    {

    }

}
