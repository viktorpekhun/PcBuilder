using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace Scraping.Infrastructure.Utilities
{
    public static class NuxtScriptWorker
    {
        public static JObject? ExtractNuxtDataFromHtml(HtmlDocument doc)
        {
            var scriptNode = doc.DocumentNode
                .SelectSingleNode("//script[contains(text(), 'window.__NUXT__')]");

            if (scriptNode == null)
                return null;
            //add null script log

            string jsCode = scriptNode.InnerText;
            jsCode = jsCode
                .Replace("“", "")
                .Replace("”", "")
                .Replace("„", "")
                .Replace("«", "")
                .Replace("»", "");
            jsCode = jsCode.Replace("‘", "").Replace("’", "");
            jsCode = jsCode.Replace("\\r\\n", "");
            jsCode = jsCode.Replace("\\\"", "");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = "C:\\Users\\vikto\\source\\repos\\viktorpekhun\\PcBuilder\\Modules\\Scraping\\Scraping.Infrastructure\\Utilities\\nuxt-extract.js",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                using (var writer = process.StandardInput)
                {
                    writer.Write(jsCode);
                }

                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Помилка у Node.js: " + errors);
                    return null;
                }

                return JObject.Parse(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C# помилка: {ex.Message}");
                return null;
            }
        }
        public static JToken? FindTokenByKey(JToken container, string key)
        {
            if (container.Type == JTokenType.Object)
            {
                foreach (var prop in (container as JObject)!)
                {
                    if (prop.Key == key)
                        return prop.Value;

                    var found = FindTokenByKey(prop.Value, key);
                    if (found != null)
                        return found;
                }
            }
            else if (container.Type == JTokenType.Array)
            {
                foreach (var item in (JArray)container)
                {
                    var found = FindTokenByKey(item, key);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }
    }
}
