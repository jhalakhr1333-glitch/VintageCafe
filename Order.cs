using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VintageCafe.Models
{
    public class Order
    {
        public int Id { get; set; }

        public bool IsDineIn { get; set; }
        public bool IsTakeAway { get; set; }

        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ NEW FIELD (Fixes Status error)
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        // ✅ Navigation property
        public List<OrderItem> Items { get; set; } = new();
    }
}
