using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureCommon.Domain.Api.Controller
{
    [MessagePackObject]
    public record BeginCharacterRegistrationResponse
    {
        [Key(0)] public bool Success { get; set; }
        [Key(1)]
        public string Ticket { get; set; } = string.Empty;
        [Key(2)]
        public string Uri { get; set; } = string.Empty;

        public BeginCharacterRegistrationResponse() { }
        public BeginCharacterRegistrationResponse(bool success, string ticket, string uri)
        {
            Success = success;
            Ticket = ticket;
            Uri = uri;
        }
    }
}
