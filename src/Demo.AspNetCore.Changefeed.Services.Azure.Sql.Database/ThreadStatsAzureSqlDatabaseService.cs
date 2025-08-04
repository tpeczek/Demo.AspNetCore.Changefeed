using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Sql.Database
{
    internal class ThreadStatsEntity : ThreadStats
    {
        public int Id { get; private set; }

        public ThreadStatsEntity()
        { }

        public ThreadStatsEntity(ThreadStats threadStats)
        {
            MaxThreads = threadStats.MaxThreads;
            MinThreads = threadStats.MinThreads;
            WorkerThreads = threadStats.WorkerThreads;
        }
    }

    internal class ThreadStatsDbContext : DbContext
    {
        internal readonly static string SCHEMA = "app";

        public DbSet<ThreadStatsEntity> ThreadStats { get; private set; }

        public ThreadStatsDbContext(DbContextOptions<ThreadStatsDbContext> options)
        : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SCHEMA);
            modelBuilder.Entity<ThreadStatsEntity>(builder =>
            {
                builder.ToTable(nameof(ThreadStats));
            });
        }
    }

    internal class ThreadStatsAzureSqlDatabaseService : IThreadStatsChangefeedDbService
    {
        private readonly AzureSqlDatabaseOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SemaphoreSlim _ensureDatabaseCreatedSemaphore = new SemaphoreSlim(1, 1);

        public ThreadStatsAzureSqlDatabaseService(IOptions<AzureSqlDatabaseOptions> options, IServiceScopeFactory serviceScopeFactory)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            await _ensureDatabaseCreatedSemaphore.WaitAsync();

            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                ThreadStatsDbContext context = serviceScope.ServiceProvider.GetRequiredService<ThreadStatsDbContext>();
                await context.Database.EnsureCreatedAsync();
            }

            _ensureDatabaseCreatedSemaphore.Release();
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new AzureSqlDatabaseChangefeed<ThreadStats>(
                ThreadStatsDbContext.SCHEMA,
                nameof(ThreadStats),
                nameof(ThreadStatsEntity.Id),
                typeof(ThreadStatsDbContext),
                _options,
                _serviceScopeFactory
            ));
        }

        public async Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                ThreadStatsDbContext context = serviceScope.ServiceProvider.GetRequiredService<ThreadStatsDbContext>();
                
                await context.AddAsync(new ThreadStatsEntity(threadStats));
                await context.SaveChangesAsync();
            }
        }
    }
}
