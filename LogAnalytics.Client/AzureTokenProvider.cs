using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;

namespace LogAnalytics.Client
{
    public class AzureTokenProvider : IAzureTokenProvider
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        
        
        public AzureTokenProvider(string tenantId, string clientId, string clientSecret)
        {
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }
        
        public async Task<string> GetAccessToken()
        {
            var response = await $"https://login.microsoftonline.com/{_tenantId}/oauth2/token"
                .PostAsync(new FormUrlEncodedContent(GetFormContent()))
                .ReceiveJson<AadTokenResponse>()
                .ConfigureAwait(false);

            return response.access_token;
        }

        private IEnumerable<KeyValuePair<string, string>> GetFormContent()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:3000/login"),
                new KeyValuePair<string, string>("resource", "https://api.loganalytics.io"),
                new KeyValuePair<string, string>("client_secret", _clientSecret)
            };
        }
    }
}