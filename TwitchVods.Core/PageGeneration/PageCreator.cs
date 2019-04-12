using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitchVods.Core.Models;
using TwitchVods.Core.Templates;
using WebMarkupMin.Core;

namespace TwitchVods.Core.PageGeneration
{
    internal class PageCreator
    {
        private readonly HtmlGenerator _htmlGenerator;
        private readonly HtmlMinifier _minifier;

        public PageCreator()
        {
            _htmlGenerator = new HtmlGenerator();
            _minifier = new HtmlMinifier();
        }

        public async Task CreateChannelPageAsync(Channel channel, Settings settings)
        {
            var model = new ChannelModel
            {
                Channel = channel,
                GoogleAnalyticsTrackingId = settings.GoogleAnalyticsTrackingId,
                TwitterHandle = settings.TwitterHandle,
            };

            var markup = await new HtmlGenerator().GenerateMarkupAsync(model, "Templates/Channel.cshtml");

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
