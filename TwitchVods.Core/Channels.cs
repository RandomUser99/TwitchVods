using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchVods.Core
{
    internal class Channels
    {
        internal static async Task<string[]> FromFileAsync(string filePath)
        {
            string fileContent;
            using (var reader = new StreamReader(filePath))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            return fileContent.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .ToList()
                            .Where(x => !x.StartsWith("//")).ToArray();
        }
    }
}
