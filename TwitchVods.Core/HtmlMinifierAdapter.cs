using System.Linq;
using WebMarkupMin.Core;

namespace TwitchVods.Core
{
    public class MinificationResult
    {
        public bool HasErrors { get; set; }
        public string MinifiedContent { get; set; }
    }

    public interface IHtmlMinifierAdapter
    {
        MinificationResult Minify(string content);
    }

    public class HtmlMinifierAdapter : IHtmlMinifierAdapter
    {
        private readonly HtmlMinifier _minifier;

        public HtmlMinifierAdapter()
        {
            _minifier = new HtmlMinifier();
        }

        public MinificationResult Minify(string content)
        {
            var result = _minifier.Minify(content);

            return new MinificationResult
            {
                HasErrors = result.Errors.Any(),
                MinifiedContent = result.MinifiedContent
            };
        }
    }
}
