namespace PcBuilderApi.Utilities.Filtering
{
    public class ResourceParameters
    {
        // Pagination
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        // Sorting
        public string OrderBy { get; set; }
        public bool Ascending { get; set; } = true;

        // Filtering - using dictionary for flexible filtering
        public Dictionary<string, string[]> Filters { get; set; } = new Dictionary<string, string[]>();

        // Search across multiple fields
        public string? SearchQuery { get; set; } = null;
    }
}
