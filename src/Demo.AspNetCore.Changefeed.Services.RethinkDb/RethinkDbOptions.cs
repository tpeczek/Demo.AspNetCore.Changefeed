namespace Demo.AspNetCore.Changefeed.Services.RethinkDb
{
    public class RethinkDbOptions
    {
        public string Hostname { get; set; }

        public int? DriverPort { get; set; }

        public int? Timeout { get; set; }
    }
}
