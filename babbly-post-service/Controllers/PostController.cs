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
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            try
            {
                // Validate user from API Gateway headers
                var userId = Request.Headers["X-User-Id"].FirstOrDefault();
                var userRoles = Request.Headers["X-User-Roles"].FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("CreatePost: No user ID found in request headers");
                    return Unauthorized(new { error = "Authentication required - no user ID in headers" });
                }
                
                // Verify the post's userId matches the authenticated user
                if (post.UserId != userId)
                {
                    _logger.LogWarning("CreatePost: User {UserId} attempted to create post for different user {PostUserId}", 
                        userId, post.UserId);
                    return StatusCode(403, new { error = "Cannot create posts for other users" });
                }
                
                // Basic authorization - authenticated users can create posts
                if (!userRoles.Contains("user") && !userRoles.Contains("admin"))
                {
                    _logger.LogWarning("CreatePost: User {UserId} lacks required role for creating posts", userId);
                    return StatusCode(403, new { error = "Insufficient permissions to create posts" });
                }
                
                // Validate content
                if (string.IsNullOrWhiteSpace(post.Content))
                {
                    return BadRequest(new { error = "Post content cannot be empty" });
                }
                
                if (post.Content.Length > 280)
                {
                    return BadRequest(new { error = "Post content cannot exceed 280 characters" });
                }

                post.Id = Guid.NewGuid();
                post.CreatedAt = DateTime.UtcNow;
                
                // Ensure UserId is set from authenticated user
                post.UserId = userId;
                
                await _mapper.InsertAsync(post);
                
                _logger.LogInformation("Created post {PostId} for user {UserId}", post.Id, userId);
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
        public async Task<IActionResult> UpdatePost(Guid id, Post post)
        {
            try
            {
                // Validate user from API Gateway headers
                var userId = Request.Headers["X-User-Id"].FirstOrDefault();
                var userRoles = Request.Headers["X-User-Roles"].FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UpdatePost: No user ID found in request headers");
                    return Unauthorized(new { error = "Authentication required - no user ID in headers" });
                }
                
                var existingPost = await _mapper.SingleOrDefaultAsync<Post>("WHERE id = ?", id);
                if (existingPost == null)
                {
                    return NotFound();
                }
                
                // Authorization: Only post owner or admin can update
                if (existingPost.UserId != userId && !userRoles.Contains("admin"))
                {
                    _logger.LogWarning("UpdatePost: User {UserId} attempted to update post {PostId} owned by {OwnerId}", 
                        userId, id, existingPost.UserId);
                    return StatusCode(403, new { error = "Can only update your own posts" });
                }
                
                // Validate content if it's being updated
                if (!string.IsNullOrWhiteSpace(post.Content))
                {
                    if (post.Content.Length > 280)
                    {
                        return BadRequest(new { error = "Post content cannot exceed 280 characters" });
                    }
                    existingPost.Content = post.Content;
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
                // Validate user from API Gateway headers
                var userId = Request.Headers["X-User-Id"].FirstOrDefault();
                var userRoles = Request.Headers["X-User-Roles"].FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("DeletePost: No user ID found in request headers");
                    return Unauthorized(new { error = "Authentication required - no user ID in headers" });
                }
                
                var post = await _mapper.SingleOrDefaultAsync<Post>("WHERE id = ?", id);
                if (post == null)
                {
                    return NotFound();
                }
                
                // Authorization: Only post owner or admin can delete
                if (post.UserId != userId && !userRoles.Contains("admin"))
                {
                    _logger.LogWarning("DeletePost: User {UserId} attempted to delete post {PostId} owned by {OwnerId}", 
                        userId, id, post.UserId);
                    return StatusCode(403, new { error = "Can only delete your own posts" });
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
