namespace PcBuilderApi.Utilities.Filtering
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public PagedResponse(IEnumerable<T> items, int count, ResourceParameters parameters)
        {
            Items = items;
            TotalCount = count;
            PageNumber = parameters.PageNumber;
            PageSize = parameters.PageSize;
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);
        }
    }
}
