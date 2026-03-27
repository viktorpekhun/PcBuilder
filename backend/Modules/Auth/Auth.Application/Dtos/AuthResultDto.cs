namespace Auth.Application.Dtos
{
    public class AuthResultDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int RefreshTokenExpirationDays { get; set; }
    }
}
