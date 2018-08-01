namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public class ThreadStats
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
