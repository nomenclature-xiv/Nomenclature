using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureClient.Network
{
    public class NetworkUtils
    {
        
        

        /// <summary>
        ///     Posts a request to the server
        /// </summary>
        public static async Task<HttpResponseMessage> PostRequest(HttpClient client, string? content, string url)
        {
            var payload = content is not null ? new StringContent(content, Encoding.UTF8, "application/json") : null;
            return await client.PostAsync(url, payload).ConfigureAwait(false);
        }
    }
}
