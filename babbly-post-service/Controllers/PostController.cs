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
                post.Id = Guid.NewGuid();
                post.CreatedAt = DateTime.UtcNow;
                await _mapper.InsertAsync(post);
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
                var existingPost = await _mapper.SingleOrDefaultAsync<Post>("WHERE id = ?", id);
                if (existingPost == null)
                {
                    return NotFound();
                }

                post.Id = id;
                await _mapper.UpdateAsync(post);
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
                var post = await _mapper.SingleOrDefaultAsync<Post>("WHERE id = ?", id);
                if (post == null)
                {
                    return NotFound();
                }

                await _mapper.DeleteAsync<Post>("WHERE id = ?", id);
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
