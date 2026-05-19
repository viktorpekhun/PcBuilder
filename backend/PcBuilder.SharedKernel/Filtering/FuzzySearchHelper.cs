using FuzzySharp;

namespace PcBuilder.SharedKernel.Filtering
{
    public static class FuzzySearchHelper
    {
        public static List<T> RankAndFilter<T>(
            IEnumerable<T> candidates,
            string query,
            Func<T, string?> nameSelector,
            int minScore = 55)
        {
            var normalizedQuery = query.ToLowerInvariant();
            return candidates
                .Select(item => (item, score: Fuzz.WeightedRatio(normalizedQuery, (nameSelector(item) ?? "").ToLowerInvariant())))
                .Where(x => x.score >= minScore)
                .OrderByDescending(x => x.score)
                .Select(x => x.item)
                .ToList();
        }
    }
}
