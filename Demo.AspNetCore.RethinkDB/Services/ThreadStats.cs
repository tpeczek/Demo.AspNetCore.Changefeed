namespace Demo.AspNetCore.RethinkDB.Services
{
    internal class ThreadStats
    {
        public int WorkerThreads { get; set; }

        public int MinThreads { get; set; }

        public int MaxThreads { get; set; }

        public override string ToString()
        {
            return $"Available: {WorkerThreads}, Active: {MaxThreads - WorkerThreads}, Min: {MinThreads}, Max: {MaxThreads}";
        }
    }
}
