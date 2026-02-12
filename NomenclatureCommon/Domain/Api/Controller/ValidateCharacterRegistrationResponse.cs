using MessagePack;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureCommon.Domain.Api.Controller
{
    [MessagePackObject]
    public record ValidateCharacterRegistrationResponse
    {
        [Key(0)]
        public string Status { get; set; } = string.Empty;
        [Key(1)]
        public string? Secret { get; set; } = null;
    }
}
