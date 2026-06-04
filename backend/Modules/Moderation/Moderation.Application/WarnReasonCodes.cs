namespace Moderation.Application
{
    public static class WarnReasonCodes
    {
        public const string Spam = "SPAM";
        public const string InappropriateContent = "INAPPROPRIATE_CONTENT";
        public const string OffensiveBehaviour = "OFFENSIVE_BEHAVIOUR";
        public const string FalseInformation = "FALSE_INFORMATION";
        public const string CopyrightViolation = "COPYRIGHT_VIOLATION";
        public const string CommunityGuidelines = "COMMUNITY_GUIDELINES";

        public static readonly IReadOnlyList<string> All =
        [
            Spam,
            InappropriateContent,
            OffensiveBehaviour,
            FalseInformation,
            CopyrightViolation,
            CommunityGuidelines,
        ];
    }
}
