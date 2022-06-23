using RazorLight;
using RazorLight.Caching;
using System.IO;
using System.Threading.Tasks;

namespace TwitchVods.Core.PageGeneration
{
    internal class HtmlGenerator
    {
        private readonly IRazorLightEngine _razorEngine;

        public HtmlGenerator()
        {
            _razorEngine = Initialise();
        }

        public async Task<string> GenerateMarkupAsync<T>(T model, string templatePath)
        {
            return await _razorEngine.CompileRenderAsync(templatePath, model);
        }

        private static IRazorLightEngine Initialise()
        {
            var currentDir = Directory.GetCurrentDirectory();

            return new RazorLightEngineBuilder()
                .UseFileSystemProject(currentDir)
                .UseCachingProvider(new MemoryCachingProvider())
                .Build();
        }
    }
}
