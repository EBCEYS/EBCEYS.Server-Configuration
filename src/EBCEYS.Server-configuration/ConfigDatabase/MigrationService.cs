using Microsoft.EntityFrameworkCore;

namespace EBCEYS.Server_configuration.ConfigDatabase
{
    /// <summary>
    /// The migration service for a single start.
    /// </summary>
    /// <param name="db">The db context.</param>
    public class MigrationService(ConfigurationDatabaseContext db) : IHostedService
    {
        private readonly CancellationTokenSource cts = new();
        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cts.Cancel();
            }
            await db.Database.MigrateAsync(cts.Token);
            await db.Database.EnsureCreatedAsync(cts.Token);
        }
        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();
            return Task.CompletedTask;
        }
    }
}
