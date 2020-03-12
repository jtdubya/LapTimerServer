using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAppPrototype.Lib
{
    public class LapTimerMessageHandler
    {
        private HttpClient m_httpClient;

        public LapTimerMessageHandler()
        {
            m_httpClient = new HttpClient();
        }

        internal void Dispose()
        {
            m_httpClient.Dispose();
        }

        public Task<HttpResponseMessage> SendRaceStartNotification(IPAddress iPAddress, int countDownMillis)
        {
            JObject json = new JObject
            {
                { "type", "start" },
                { "countDowmMillis", countDownMillis }
            };
            UriBuilder uriBuilder = new UriBuilder()
            {
                Host = iPAddress.ToString(),
                Scheme = Uri.UriSchemeHttp
            };
            return m_httpClient.PostAsync(uriBuilder.ToString(), new StringContent(json.ToString()));
        }
    }
}