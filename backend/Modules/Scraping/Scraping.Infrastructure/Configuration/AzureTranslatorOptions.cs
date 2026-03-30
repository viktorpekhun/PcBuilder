namespace Scraping.Infrastructure.Configuration
{
    public class AzureTranslatorOptions
    {
        public const string SectionName = "AzureTranslator";

        public string Endpoint { get; set; } = "https://api.cognitive.microsofttranslator.com";
        public string Key { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
