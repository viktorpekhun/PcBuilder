using System.Text.RegularExpressions;

namespace PcBuilder.SharedKernel.Services
{
    public class ProfanityFilterService : IProfanityFilterService
    {
        private static readonly HashSet<string> BannedWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // English
            "fuck", "shit", "ass", "bitch", "bastard", "damn", "crap", "piss",
            "cock", "dick", "pussy", "cunt", "whore", "slut", "nigger", "nigga",
            "faggot", "fag", "retard", "idiot", "moron", "asshole", "motherfucker",

            // Ukrainian/Russian
            "хуй", "піздець", "пизда", "єбать", "їбать", "блядь", "блять", "бля",
            "сука", "єбаний", "їбаний", "мудак", "залупа", "шльоха", "шлюха",
            "єбана", "їбана", "пиздець", "курва", "мразь", "падла", "ублюдок",
            "сволота", "гандон", "мандавошка", "піздабол", "їбальник"
        };

        public bool ContainsProfanity(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (var word in BannedWords)
            {
                var pattern = $@"\b{Regex.Escape(word)}\b";
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }
}