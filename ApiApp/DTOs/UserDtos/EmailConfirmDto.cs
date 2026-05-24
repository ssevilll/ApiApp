namespace ApiApp.DTOs.UserDtos
{
    public class EmailConfirmDto
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
