using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMarkupMin.Core;

namespace TwitchVods.Core.PageGeneration
{
    using Models;
    using Templates;

    internal class PageCreator
    {
        private readonly HtmlGenerator _htmlGenerator;
        private readonly HtmlMinifier _minifier;

        public PageCreator(HtmlGenerator htmlGenerator, HtmlMinifier minifier)
        {
            _htmlGenerator = htmlGenerator;
            _minifier = minifier;
        }

        public async Task CreateChannelPageAsync(Channel channel, Settings settings)
        {
            if (channel.TotalVideoCount == 0)
            {
                return;
            }

            var model = new ChannelModel
            {
                Channel = channel,
                GoogleAnalyticsTrackingId = settings.GoogleAnalyticsTrackingId,
                TwitterHandle = settings.TwitterHandle,
                RedditUsername = settings.RedditUsername
            };

            var markup = await _htmlGenerator.GenerateMarkupAsync(model, "Templates/Channel.cshtml");

            var compressionResult = _minifier.Minify(markup);

            if (compressionResult.Errors.Any())
                return;

            await WriteToFileAsync($"{settings.OutputDir}/{channel.Name.ToLower()}.html", compressionResult.MinifiedContent);
        }

        public async Task CreateIndexPageAsync(string[] channels, string outputPath)
        {
            var markup = await _htmlGenerator.GenerateMarkupAsync(new IndexModel
            {
                Channels = channels
            }, "Templates/Index.cshtml");

            await WriteToFileAsync($"{outputPath}/index.html", markup);
        }

        private static async Task WriteToFileAsync(string outputPath, string fileContent)
        {
            using (var outputFile = new StreamWriter(outputPath))
            {
                await outputFile.WriteAsync(fileContent);
                await outputFile.FlushAsync();
            }
        }
    }
}
