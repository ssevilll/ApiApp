using Microsoft.AspNetCore.Http;

namespace ApiApp.DTOs.OrganizerDtos
{
    public class OrganizerCreateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public IFormFile? Logo { get; set; }
    }
}
