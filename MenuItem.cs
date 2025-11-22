using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VintageCafe.Models
{
    [Table("MenuItems")] // Ensures EF maps to the correct SQL table
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // ? Prevents crash if column missing and gives default
        [Required]
        [Column(TypeName = "bit")]
        public bool IsSnack { get; set; } = false;

        [MaxLength(50)]
        public string Category { get; set; } = "General";

        [MaxLength(200)]
        public string ImagePath { get; set; } = "/images/foodsimg/placeholder.png";
    }
}
