using System.ComponentModel.DataAnnotations;

namespace babbly_post_service.DTOs
{
    public class CreatePostDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(280, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 280 characters")]
        public string Content { get; set; } = string.Empty;
    }
} 