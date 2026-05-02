using FuzzySharp;
using Scraping.Application.Interfaces;
using System.Text.RegularExpressions;

namespace Scraping.Infrastructure.Services
{
    public class FuzzyStringMatchingService : IStringMatchingService
    {
        private static readonly Regex VendorPrefixPattern = new(
            @"\b(intel®?|amd|nvidia®?|geforce|radeon|core™?|ryzen|threadripper)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PackagingSuffixPattern = new(
            @"\s*\((tray|box|oem|mpk|bulk)\)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TrademarkPattern = new(
            @"[®™©]",
            RegexOptions.Compiled);

        public StringMatch<T>? FindBestMatch<T>(
            string query,
            IEnumerable<T> candidates,
            Func<T, string> selector,
            int minScore = 80)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            var normalizedQuery = Normalize(query);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
                return null;

            StringMatch<T>? best = null;
            int bestCandidateLength = 0;

            foreach (var candidate in candidates)
            {
                var name = selector(candidate);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var normalizedCandidate = Normalize(name);
                var score = Fuzz.TokenSetRatio(normalizedQuery, normalizedCandidate);

                if (score >= minScore && (best == null || score > best.Score ||
                    (score == best.Score && normalizedCandidate.Length > bestCandidateLength)))
                {
                    best = new StringMatch<T>(candidate, score);
                    bestCandidateLength = normalizedCandidate.Length;
                }
            }

            return best;
        }

        private static string Normalize(string input)
        {
            var result = TrademarkPattern.Replace(input, "");
            result = PackagingSuffixPattern.Replace(result, "");
            result = VendorPrefixPattern.Replace(result, "");
            result = Regex.Replace(result, @"\s+", " ").Trim().ToLowerInvariant();
            return result;
        }
    }
}
