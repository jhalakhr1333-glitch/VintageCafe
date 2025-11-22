using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VintageCafe.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem { get; set; } = default!;

        [Required]
        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        public decimal LineTotal { get; set; }

        public int OrderId { get; set; }
    }
}
