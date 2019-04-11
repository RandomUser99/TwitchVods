using RazorLight;
using RazorLight.Caching;
using System.IO;
using System.Threading.Tasks;
using TwitchVods.Core.Templates;

namespace TwitchVods.Core
{
    internal class ChannelGenerator
    {
        private readonly IRazorLightEngine _razorEngine;
        private bool TemplateCached => _razorEngine.TemplateCache.Contains(TemplatePath);
        private static string TemplatePath => "Templates/Channel.cshtml";

        public ChannelGenerator()
        {
            _razorEngine = GetRazorEngine();
        }

        public async Task<string> GenerateMarkupAsync(ChannelModel model)
        {
            if (!TemplateCached)
                await PrecompileTemplateAsync(model);

            return await _razorEngine.CompileRenderAsync(TemplatePath, model);
        }

        private async Task PrecompileTemplateAsync(ChannelModel model)
        {
            // Pre-compiling the templates is a must for performance reasons.
            // It reduces the time to render from a template each time from  > 5secs to 30ms 
            // after the initial compilation has executed.
            await _razorEngine.CompileRenderAsync(TemplatePath, model);
        }

        private static IRazorLightEngine GetRazorEngine()
        {
            var currentDir = Directory.GetCurrentDirectory();

            return new RazorLightEngineBuilder()
                .UseFilesystemProject(currentDir)
                .UseCachingProvider(new MemoryCachingProvider())
                .Build();
        }
    }
}
