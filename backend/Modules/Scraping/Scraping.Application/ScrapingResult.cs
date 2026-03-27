using Components.Domain.Entities;

namespace Scraping.Application
{
    public class ScrapingResult<T> where T : class
    {
        public T? Component { get; }
        public List<Store> Stores { get; }
        public List<ProductOffer> Offers { get; }

        public ScrapingResult(T? component, List<Store> stores, List<ProductOffer> offers)
        {
            Component = component;
            Stores = stores;
            Offers = offers;
        }
    }
}
