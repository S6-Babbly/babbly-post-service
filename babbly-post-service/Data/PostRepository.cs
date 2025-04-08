using babbly_post_service.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace babbly_post_service.Data
{
    public class PostRepository : CassandraRepository<Post>
    {
        public PostRepository(
            CassandraContext context,
            ILogger<PostRepository> logger) 
            : base(context, logger, "posts")
        {
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
        {
            return await QueryAsync("WHERE user_id = ? ALLOW FILTERING", userId);
        }

        public async Task<IEnumerable<Post>> GetLatestPostsAsync(int limit = 20)
        {
            return await QueryAsync("ORDER BY created_at DESC LIMIT ?", limit);
        }

        public async Task IncrementLikesAsync(Guid id)
        {
            await ExecuteAsync("UPDATE posts SET likes = likes + 1 WHERE id = ?", id);
        }

        public async Task<IEnumerable<Post>> GetPopularPostsAsync(int limit = 20)
        {
            return await QueryAsync("ORDER BY likes DESC LIMIT ?", limit);
        }
    }
} 