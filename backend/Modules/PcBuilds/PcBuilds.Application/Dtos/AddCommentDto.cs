namespace PcBuilds.Application.Dtos
{
    public class AddCommentDto
    {
        public string Text { get; set; } = string.Empty;
        public decimal Rating { get; set; }
    }
}