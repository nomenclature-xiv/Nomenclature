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
        public string Status { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}
