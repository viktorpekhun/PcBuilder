namespace Auth.Application.Dtos
{
    public class UpdateProfileDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Bio { get; set; }
    }
}
