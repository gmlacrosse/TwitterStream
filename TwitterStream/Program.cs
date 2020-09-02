using System;

namespace TwitterStream
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting TWitter Sample Stream Monitor Press Ctrl-C to exit.");
            MonitorTWeets();
        }

        private static void MonitorTWeets()
        {
            var monitor = new TwitterMonitor();
            monitor.Start();
        }
    }
}
