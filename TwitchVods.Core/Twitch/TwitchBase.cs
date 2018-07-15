using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;

namespace TwitchVods.Core.Twitch
{
    public abstract class TwitchBase
    {
        private readonly string _twitchApiClientId;

        protected enum TwitchApiVersion
        {
            v3 = 3,
            v5 = 5
        }

        protected string ChannelUserId;

        protected TwitchBase(string twitchApiClientId)
        {
            _twitchApiClientId = twitchApiClientId;
        }

        protected void SetUserId(string channelName)
        {
            if (!string.IsNullOrWhiteSpace(ChannelUserId))
                return;

            var apiEndpoint = $"https://api.twitch.tv/kraken/users?login={channelName}&api_version=5";

            var request = CreateWebRequest(apiEndpoint, TwitchApiVersion.v5);

            var webResponse = request.GetResponse();

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                dynamic jsonData = JObject.Parse(reader.ReadToEnd());
                ChannelUserId = jsonData["users"][0]["_id"].ToString();
            }
        }

        protected HttpWebRequest CreateWebRequest(string apiEndpoint, TwitchApiVersion apiVersion)
        {
            var request = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            // Need to specify the version of the API to use.
            request.Accept = $@"application/vnd.twitchtv.{apiVersion}+json";

            // Need to specify the client ID https://blog.twitch.tv/client-id-required-for-kraken-api-calls-afbb8e95f843#.j496nqkhq  
            request.Headers.Add("Client-ID", _twitchApiClientId);

            return request;
        }
    }
}
