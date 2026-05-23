namespace ApiApp.DTOs.OrganizerDtos
{
    public class OrganizerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? LogoUrl { get; set; }
    }
}
