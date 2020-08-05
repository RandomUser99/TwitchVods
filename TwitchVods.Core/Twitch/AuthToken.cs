using System;

namespace TwitchVods.Core.Twitch
{
    internal class AuthToken
    {
        private DateTime _dateCreated;

        public string AccessToken { get; private set; }
        public int ExpiresInSeconds { get; private set; }
        public bool HasExpired => DateTime.Now >= _dateCreated.AddSeconds(ExpiresInSeconds);

        public static AuthToken Create(string token, int expiresIn)
        {
            return new AuthToken
            {
                AccessToken = token,
                ExpiresInSeconds = expiresIn,
                _dateCreated = DateTime.Now
            };
        }
    }
}
