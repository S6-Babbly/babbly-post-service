using System.ComponentModel.DataAnnotations;

namespace babbly_post_service.DTOs
{
    public class CreatePostDto
    {
        [Required]
        [StringLength(280, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 280 characters")]
        public string Content { get; set; } = string.Empty;

        [Url(ErrorMessage = "MediaUrl must be a valid URL")]
        public string? MediaUrl { get; set; }
    }
} 