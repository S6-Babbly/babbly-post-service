using babbly_post_service.Models;
using Cassandra;
using Cassandra.Mapping;

namespace babbly_post_service.Data
{
    public class PostRepository
    {
        private readonly CassandraContext _context;
        private readonly IMapper _mapper;

        public PostRepository(CassandraContext context)
        {
            _context = context;
            _mapper = context.Mapper;
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await _mapper.FirstOrDefaultAsync<Post>("WHERE id = ?", id);
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _mapper.FetchAsync<Post>("SELECT * FROM posts");
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
        {
            return await _mapper.FetchAsync<Post>("WHERE user_id = ? ALLOW FILTERING", userId);
        }

        public async Task<Post> CreateAsync(Post post)
        {
            await _mapper.InsertAsync(post);
            return post;
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            await _mapper.UpdateAsync(post);
            return post;
        }

        public async Task DeleteAsync(Guid id)
        {
            await _mapper.DeleteAsync<Post>("WHERE id = ?", id);
        }

        public async Task IncrementLikesAsync(Guid id)
        {
            await _context.Session.ExecuteAsync(
                "UPDATE posts SET likes = likes + 1 WHERE id = ?", 
                id);
        }
    }
} 