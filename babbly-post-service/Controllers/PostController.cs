using babbly_post_service.Data;
using babbly_post_service.DTOs;
using babbly_post_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace babbly_post_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostController> _logger;

        public PostController(ApplicationDbContext context, ILogger<PostController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts()
        {
            _logger.LogInformation("Getting all posts");
            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => MapPostToDto(p))
                .ToListAsync();

            return Ok(posts);
        }

        // GET: api/Post/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(int id)
        {
            _logger.LogInformation("Getting post with ID: {PostId}", id);
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                _logger.LogWarning("Post with ID: {PostId} not found", id);
                return NotFound();
            }

            return Ok(MapPostToDto(post));
        }

        // GET: api/Post/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByUser(int userId)
        {
            _logger.LogInformation("Getting posts for user with ID: {UserId}", userId);
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => MapPostToDto(p))
                .ToListAsync();

            return Ok(posts);
        }

        // POST: api/Post
        [HttpPost]
        public async Task<ActionResult<PostDto>> CreatePost(CreatePostDto createPostDto)
        {
            // Validate the post content
            if (string.IsNullOrWhiteSpace(createPostDto.Content))
            {
                return BadRequest("Post content cannot be empty");
            }

            if (createPostDto.Content.Length > 280)
            {
                return BadRequest("Post content cannot exceed 280 characters");
            }

            _logger.LogInformation("Creating new post for user: {UserId}", createPostDto.UserId);
            
            // Create a new post with server-generated timestamp
            var post = new Post
            {
                UserId = createPostDto.UserId,
                Content = createPostDto.Content,
                CreatedAt = DateTime.UtcNow,
                Likes = 0 // Explicitly initialize likes to 0
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Return the created post with all server-generated fields
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, MapPostToDto(post));
        }

        // PUT: api/Post/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, UpdatePostDto updatePostDto)
        {
            // Validate the post content
            if (updatePostDto.Content != null && updatePostDto.Content.Length > 280)
            {
                return BadRequest("Post content cannot exceed 280 characters");
            }

            _logger.LogInformation("Updating post with ID: {PostId}", id);
            
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                _logger.LogWarning("Post with ID: {PostId} not found", id);
                return NotFound();
            }

            if (updatePostDto.Content != null)
            {
                post.Content = updatePostDto.Content;
            }

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/Post/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            _logger.LogInformation("Deleting post with ID: {PostId}", id);
            
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                _logger.LogWarning("Post with ID: {PostId} not found", id);
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Post/5/like
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(int id)
        {
            _logger.LogInformation("Liking post with ID: {PostId}", id);
            
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                _logger.LogWarning("Post with ID: {PostId} not found", id);
                return NotFound();
            }

            // Increment likes count
            post.Likes += 1;
            await _context.SaveChangesAsync();

            return Ok(new { likes = post.Likes });
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        // Helper method to map Post entity to PostDto
        private static PostDto MapPostToDto(Post post)
        {
            return new PostDto
            {
                Id = post.Id,
                UserId = post.UserId,
                Content = post.Content,
                Likes = post.Likes,
                CreatedAt = post.CreatedAt,
                // Add any additional formatting or computed properties here
                TimeAgo = FormatTimeAgo(post.CreatedAt)
            };
        }

        // Helper method to format time ago string on the server side
        private static string FormatTimeAgo(DateTime dateTime)
        {
            var now = DateTime.UtcNow;
            var diffInSeconds = (int)(now - dateTime).TotalSeconds;
            
            if (diffInSeconds < 60)
            {
                return $"{diffInSeconds}s";
            }
            
            var diffInMinutes = (int)(diffInSeconds / 60);
            if (diffInMinutes < 60)
            {
                return $"{diffInMinutes}m";
            }
            
            var diffInHours = (int)(diffInMinutes / 60);
            if (diffInHours < 24)
            {
                return $"{diffInHours}h";
            }
            
            var diffInDays = (int)(diffInHours / 24);
            if (diffInDays < 7)
            {
                return $"{diffInDays}d";
            }
            
            var diffInWeeks = (int)(diffInDays / 7);
            if (diffInWeeks < 4)
            {
                return $"{diffInWeeks}w";
            }
            
            var diffInMonths = (int)(diffInDays / 30);
            if (diffInMonths < 12)
            {
                return $"{diffInMonths}mo";
            }
            
            var diffInYears = (int)(diffInDays / 365);
            return $"{diffInYears}y";
        }
    }
}
