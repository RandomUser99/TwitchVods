using System.Threading.Tasks;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Twitch
{
    internal interface ITwitchClient
    {
        Task<Channel> GetChannelVideosAsync();
    }
}