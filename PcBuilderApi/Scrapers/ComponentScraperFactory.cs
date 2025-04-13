namespace PcBuilderApi.Scrapers
{
    public class ComponentScraperFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ComponentScraperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IComponentScraper<T>? GetScraper<T>() where T : class
        {
            return _serviceProvider.GetService<IComponentScraper<T>>();
        }
    }
}
