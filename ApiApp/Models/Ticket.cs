using ApiApp.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiApp.Models
{
    public class Ticket : BaseEntity
    {
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int QuantityAvailable { get; set; }
    }
}
