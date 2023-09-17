namespace Demo.AspNetCore.Changefeed.Services.RethinkDB
{
    internal interface IRethinkDbSingletonProvider
    {
        RethinkDb.Driver.RethinkDB RethinkDbSingleton { get; }

        RethinkDb.Driver.Net.Connection RethinkDbConnection { get; }
    }
}
