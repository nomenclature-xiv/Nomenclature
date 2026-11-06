using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureCommon.Domain.Api.Controller.Oauth
{
    [MessagePackObject]
    public record BeginCharacterRegistrationResponseOauth
    {
        [Key(0)] public bool Success { get; set; }
        [Key(1)]
        public string Ticket { get; set; } = string.Empty;
        [Key(2)]
        public string Uri { get; set; } = string.Empty;

        public BeginCharacterRegistrationResponseOauth() { }
        public BeginCharacterRegistrationResponseOauth(bool success, string ticket, string uri)
        {
            Success = success;
            Ticket = ticket;
            Uri = uri;
        }
    }
}
