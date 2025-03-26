using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace babbly_post_service.Models
{
    public class Post
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int UserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public int Likes { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 