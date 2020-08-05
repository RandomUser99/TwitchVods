using System;

namespace TwitchVods.Core.Twitch
{
    internal class TwitchAuthToken
    {
        public string AccessToken { get; private set; }
        public int ExpiresInSeconds { get; private set; }
        public DateTime DateCreated { get; private set; }
        public bool HasExpired => DateTime.Now >= DateCreated.AddSeconds(ExpiresInSeconds);

        public static TwitchAuthToken Create(string token, int expiresIn)
        {
            return new TwitchAuthToken
            {
                AccessToken = token,
                ExpiresInSeconds = expiresIn,
                DateCreated = DateTime.Now
            };
        }
    }
}
