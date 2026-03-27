namespace PcBuilder.SharedKernel.Filtering
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

        public string ToCacheKey()
        {
            var parts = new List<string>
            {
                $"p{PageNumber}",
                $"s{PageSize}",
                $"o{OrderBy ?? "default"}",
                $"a{Ascending}"
            };

            if (!string.IsNullOrEmpty(SearchQuery))
                parts.Add($"q{SearchQuery}");

            if (Filters is { Count: > 0 })
            {
                foreach (var kvp in Filters.OrderBy(k => k.Key))
                    parts.Add($"f{kvp.Key}={string.Join(",", kvp.Value.OrderBy(v => v))}");
            }

            return string.Join(":", parts);
        }
    }
}
