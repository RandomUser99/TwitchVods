using System.Threading.Tasks;

namespace TwitchVods.Core.Output
{
    public interface IOutputWriter
    {
        Task WriteOutputAsync();
    }
}