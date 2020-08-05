using System;

namespace TwitchVods.Core.Twitch
{
    internal class TwitchAuthToken
    {
        private DateTime _dateCreated;

        public string AccessToken { get; private set; }
        public int ExpiresInSeconds { get; private set; }
        public bool HasExpired => DateTime.Now >= _dateCreated.AddSeconds(ExpiresInSeconds);

        public static TwitchAuthToken Create(string token, int expiresIn)
        {
            return new TwitchAuthToken
            {
                AccessToken = token,
                ExpiresInSeconds = expiresIn,
                _dateCreated = DateTime.Now
            };
        }
    }
}
