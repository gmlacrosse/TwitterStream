using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Tweetinvi;

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
