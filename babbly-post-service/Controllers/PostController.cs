using babbly_post_service.Data;
using babbly_post_service.DTOs;
using babbly_post_service.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Cassandra.Mapping;
using Cassandra;

namespace babbly_post_service.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly Cassandra.ISession _session;
        private readonly ILogger<PostController> _logger;

        public PostController(CassandraContext context, ILogger<PostController> logger)
        {
            _mapper = context.Mapper;
            _session = context.Session;
            _logger = logger;
        }

        // GET: api/Post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            try
            {
                var posts = await _mapper.FetchAsync<Post>("SELECT * FROM posts");
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Post/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(Guid id)
        {
            try
            {
                var post = await _mapper.SingleOrDefaultAsync<Post>("WHERE id = ?", id);
                if (post == null)
                {
                    return NotFound();
                }
                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Post/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPostsByUser(string userId)
        {
            _logger.LogInformation("Getting posts for user with ID: {UserId}", userId);
            var posts = await _mapper.FetchAsync<Post>("WHERE user_id = ?", userId);
            return Ok(posts.Select(p => MapPostToDto(p)));
        }

        // GET: api/Post/popular
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPopularPosts()
        {
            _logger.LogInformation("Getting popular posts");
            var posts = await _mapper.FetchAsync<Post>("WHERE is_popular = true");
            return Ok(posts.Select(p => MapPostToDto(p)));
        }

        // POST: api/Post
        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost(CreatePostDto postDto)
        {
            try
            {
                // Validate content
                if (string.IsNullOrWhiteSpace(postDto.Content))
                {
                    return BadRequest(new { error = "Post content cannot be empty" });
                }
                
                if (postDto.Content.Length > 280)
                {
                    return BadRequest(new { error = "Post content cannot exceed 280 characters" });
                }

                // Get authenticated user ID from JWT headers (forwarded by API Gateway)
                var userId = Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { error = "Authentication required. User ID not found in token." });
                }

                // Create post with authenticated user
                var post = new Post
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Content = postDto.Content.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                // Set optional fields if provided
                if (!string.IsNullOrWhiteSpace(postDto.MediaUrl))
                {
                    post.Image = postDto.MediaUrl;
                }
                
                await _mapper.InsertAsync(post);
                
                _logger.LogInformation("Created post {PostId} for authenticated user {UserId}", post.Id, userId);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Post/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, UpdatePostDto postDto)
        {
            try
            {
                // Get authenticated user ID from JWT headers
                var userId = Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { error = "Authentication required. User ID not found in token." });
                }

                var existingPost = await _mapper.SingleOrDefaultAsync<Post>("WHERE id = ?", id);
                if (existingPost == null)
                {
                    return NotFound(new { error = "Post not found" });
                }
                
                // Ensure user can only edit their own posts
                if (existingPost.UserId != userId)
                {
                    return Forbid("You can only edit your own posts");
                }
                
                // Validate and update content if provided
                if (!string.IsNullOrWhiteSpace(postDto.Content))
                {
                    if (postDto.Content.Length > 280)
                    {
                        return BadRequest(new { error = "Post content cannot exceed 280 characters" });
                    }
                    existingPost.Content = postDto.Content.Trim();
                }

                // Update other fields if provided
                if (postDto.MediaUrl != null)
                {
                    existingPost.Image = postDto.MediaUrl;
                }

                existingPost.Id = id;
                await _mapper.UpdateAsync(existingPost);
                
                _logger.LogInformation("Updated post {PostId} by user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Post/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            try
            {
                // Get authenticated user ID from JWT headers
                var userId = Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { error = "Authentication required. User ID not found in token." });
                }

                var post = await _mapper.SingleOrDefaultAsync<Post>("WHERE id = ?", id);
                if (post == null)
                {
                    return NotFound(new { error = "Post not found" });
                }

                // Ensure user can only delete their own posts
                if (post.UserId != userId)
                {
                    return Forbid("You can only delete your own posts");
                }

                await _mapper.DeleteAsync<Post>("WHERE id = ?", id);
                
                _logger.LogInformation("Deleted post {PostId} by user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // Helper method to map Post entity to PostDto
        private static PostDto MapPostToDto(Post post)
        {
            return new PostDto
            {
                Id = post.Id,
                UserId = post.UserId,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                TimeAgo = FormatTimeAgo(post.CreatedAt)
            };
        }

        // Helper method to format time ago string on the server side
        private static string FormatTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;

            if (span.TotalDays > 365)
            {
                var years = (int)(span.TotalDays / 365);
                return $"{years}y ago";
            }
            if (span.TotalDays > 30)
            {
                var months = (int)(span.TotalDays / 30);
                return $"{months}mo ago";
            }
            if (span.TotalDays > 1)
            {
                return $"{(int)span.TotalDays}d ago";
            }
            if (span.TotalHours > 1)
            {
                return $"{(int)span.TotalHours}h ago";
            }
            if (span.TotalMinutes > 1)
            {
                return $"{(int)span.TotalMinutes}m ago";
            }

            return "Just now";
        }
    }
}
