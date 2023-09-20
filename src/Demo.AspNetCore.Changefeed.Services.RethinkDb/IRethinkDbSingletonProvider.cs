namespace Demo.AspNetCore.Changefeed.Services.RethinkDb
{
    internal interface IRethinkDbSingletonProvider
    {
        global::RethinkDb.Driver.RethinkDB RethinkDbSingleton { get; }

        global::RethinkDb.Driver.Net.Connection RethinkDbConnection { get; }
    }
}
