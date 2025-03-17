using System.ComponentModel.DataAnnotations;

namespace babbly_post_service.DTOs
{
    public class UpdatePostDto
    {
        [StringLength(280, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 280 characters")]
        public string? Content { get; set; }
    }
} 