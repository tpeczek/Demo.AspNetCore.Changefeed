using System;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Sql.Database
{
    public class AzureSqlDatabaseOptions
    {
        public TimeSpan PollInterval { get; internal set; }

        public string? ConnectionString { get; internal set; }
    }
}
