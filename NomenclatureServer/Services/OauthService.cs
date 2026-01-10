using NomenclatureServer.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using System.Net.Http.Headers;
using NomenclatureCommon.Domain;
using Microsoft.IdentityModel.Tokens;
using NomenclatureServer.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NomenclatureCommon.Domain.Exceptions;
using Microsoft.Extensions.Hosting;

namespace NomenclatureServer.Services
{
    public class OauthService(HttpClient client, Configuration config) : IHostedService
    {
        private const string url = "https://xivauth.net";
        private const string client_id = "AiasXFDGFGc2pVPk6XvEgY5WPAn7x3PMgN-4yJ49VD4";
        private Dictionary<string, Character?> loginEphemerals = new();

        public string GetAuthorizationUrl(string ticket)
        {
            var builder = new UriBuilder(url);
            builder.Path = "/oauth/authorize";
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["response_type"] = "code";
            query["client_id"] = client_id;
            query["scope"] = "character";
#if DEBUG
            query["redirect_uri"] = "https://localhost:5006/registration/callback";
#else
            query["redirect_uri"] = "https://foxitsvc.com:5007/registration/callback";
#endif
            query["state"] = ticket;
            builder.Query = query.ToString();
            return builder.Uri.ToString();
        }

        public string GetSetTicket()
        {
            var ticket = Guid.NewGuid().ToString();
            loginEphemerals[ticket] = null;
            return ticket;
        }

        public async Task<string?> GetBearerToken(string code, string path)
        {
            TokenRequestModel data = new()
            {
                grant_type = "authorization_code",
                client_id = client_id,
                client_secret = config.ClientSecret,
                code = code,
                redirect_uri = path
            };
            var res = await client.PostAsJsonAsync<TokenRequestModel>($"{url}/oauth/token", data);
            string text = await res.Content.ReadAsStringAsync();
            var resmodel = JsonSerializer.Deserialize<TokenResponseModel>(text);
            if (resmodel is null) return null;
            return resmodel.access_token;
        }

        public async Task GetCharacter(string token, string state)
        {
            var builder = new UriBuilder(url);
            builder.Path = "/api/v1/characters";
            builder.Port = -1;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = builder.Uri
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            var res = await client.SendAsync(request);
            string text = await res.Content.ReadAsStringAsync();
            var resmodel = JsonSerializer.Deserialize<CharacterResponseModel[]>(text);
            if (resmodel is null || resmodel.Length < 1) return;
            loginEphemerals[state] = new Character(resmodel[0].name, resmodel[0].home_world);
        }

        public JwtSecurityToken? ValidateTicket(Character character, string ticket)
        {
            loginEphemerals.TryGetValue(ticket, out var bound);
            if (bound is null) return null;
            
            if (!character.Equals(bound))
            {
                loginEphemerals.Remove(ticket);
                throw new CharacterNotMatchingException("The character that was authorized via XIVAuth does not match your current character. Please try again, proceeding through the login steps with the correct character.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SigningKey));
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([new Claim(AuthClaimType.SyncCode, bound.ToString())]),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.UtcNow.AddHours(4)
            };

            return new JwtSecurityTokenHandler().CreateJwtSecurityToken(descriptor);
        }

        public Task StartAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
