using Cassandra;
using Cassandra.Mapping;
using babbly_post_service.Models;

namespace babbly_post_service.Data
{
    public class CassandraContext
    {
        private readonly Cluster _cluster;
        private readonly ISession _session;
        private readonly IMapper _mapper;

        public CassandraContext(IConfiguration configuration)
        {
            var hosts = configuration["CassandraHosts"]?.Split(',') 
                ?? throw new ArgumentNullException("CassandraHosts configuration missing");
            var keyspace = configuration["CassandraKeyspace"] 
                ?? throw new ArgumentNullException("CassandraKeyspace configuration missing");
            var username = configuration["CassandraUsername"];
            var password = configuration["CassandraPassword"];

            var clusterBuilder = Cluster.Builder()
                .AddContactPoints(hosts);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                clusterBuilder.WithCredentials(username, password);
            }

            _cluster = clusterBuilder.Build();
            _session = _cluster.Connect(keyspace);
            
            // Configure mappings
            MappingConfiguration mappingConfig = new MappingConfiguration();
            mappingConfig.Define(
                new Map<Post>()
                    .TableName("posts")
                    .PartitionKey(p => p.Id)
                    .Column(p => p.Id, cm => cm.WithName("id").WithDbType<Guid>())
                    .Column(p => p.UserId, cm => cm.WithName("user_id"))
                    .Column(p => p.Content, cm => cm.WithName("content"))
                    .Column(p => p.Likes, cm => cm.WithName("likes"))
                    .Column(p => p.CreatedAt, cm => cm.WithName("created_at"))
            );

            _mapper = new Mapper(_session, mappingConfig);
        }

        public IMapper Mapper => _mapper;
        public ISession Session => _session;

        public void Dispose()
        {
            _session?.Dispose();
            _cluster?.Dispose();
        }
    }
} 