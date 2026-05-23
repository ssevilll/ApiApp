using ApiApp.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace ApiApp.Models
{
    public class Organizer : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [MaxLength(20)]
        public string? Phone { get; set; }

        public string? LogoUrl { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
