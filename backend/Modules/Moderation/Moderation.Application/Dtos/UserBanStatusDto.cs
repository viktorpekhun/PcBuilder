namespace Moderation.Application.Dtos
{
    public class UserBanStatusDto
    {
        public DateTime? CommentBanUntil { get; set; }
        public DateTime? PostBanUntil { get; set; }
        public bool IsCommentBanned { get; set; }
        public bool IsPostBanned { get; set; }
        public List<WarningDto> RecentWarnings { get; set; } = [];
    }
}
