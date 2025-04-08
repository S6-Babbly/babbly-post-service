using System;
using System.ComponentModel.DataAnnotations;

namespace babbly_post_service.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Location { get; set; }
        public string? Image { get; set; }
    }
} 