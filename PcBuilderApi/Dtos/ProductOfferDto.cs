namespace PcBuilderApi.Dtos
{
    public class ProductOfferDto
    {
        public decimal Price { get; set; }
        public string ProductOfferUrl { get; set; } = string.Empty;
        public StoreDto Store { get; set; } = null!;
    }
}
