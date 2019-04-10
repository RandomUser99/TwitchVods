using RazorLight;
using RazorLight.Caching;
using System.IO;
using System.Threading.Tasks;

namespace TwitchVods.Core
{
    internal class IndexGenerator
    {
        private readonly IRazorLightEngine _razorEngine;

        public IndexGenerator()
        {
            _razorEngine = GetRazorEngine();
        }

        public async Task<string> GenerateMarkupAsync(Templates.IndexModel indexModel)
        {
            var currentDir = Directory.GetCurrentDirectory();
            return await _razorEngine.CompileRenderAsync("Templates/Index.cshtml",  indexModel);
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
