using System;
using System.ComponentModel.DataAnnotations;

namespace VintageCafe.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string UserName { get; set; } = "";

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required, StringLength(500)]
        public string Comment { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ New optional fields for Admin moderation
        public bool IsApproved { get; set; } = true;  // admin can hide/show reviews later
        public string? AdminReply { get; set; }       // optional reply by admin (if needed)
    }
}
