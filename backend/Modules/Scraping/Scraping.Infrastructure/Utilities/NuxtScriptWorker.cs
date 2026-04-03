using HtmlAgilityPack;
using Jint;
using Newtonsoft.Json.Linq;
using System.Text.Json;

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

            string jsCode = scriptNode.InnerText;
            jsCode = jsCode
                .Replace("\u201C", "")
                .Replace("\u201D", "")
                .Replace("\u201E", "")
                .Replace("\u00AB", "")
                .Replace("\u00BB", "");
            jsCode = jsCode.Replace("\u2018", "").Replace("\u2019", "");
            jsCode = jsCode.Replace("\\r\\n", "");
            jsCode = jsCode.Replace("\\\"", "");

            try
            {
                var engine = new Engine(options =>
                {
                    options.TimeoutInterval(TimeSpan.FromSeconds(5));
                    options.LimitMemory(50_000_000);
                    options.MaxStatements(100_000);
                });

                engine.Execute("var window = {};");
                engine.Execute(jsCode);

                var nuxtValue = engine.Evaluate("JSON.stringify(window.__NUXT__)");
                var json = nuxtValue.AsString();

                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Jint error: {ex.Message}");
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
