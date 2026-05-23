using ApiApp.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiApp.Models
{
    public class Event : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;

        public string? BannerImageUrl { get; set; }

        public int OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
