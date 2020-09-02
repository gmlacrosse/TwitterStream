using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TwitterStream
{
    public class Counters
    {
        private DateTime StartDateTime = DateTime.UtcNow;
        public int Count { get; set; }
        public int WithHashTags { get; set; }
        public int WithUrls { get; set; }
        public int WithMedia { get; set; }
        public int WithEmojis { get; set; }

        public Counters()
        {
            Count = 0;
        }

        public string GetStatsAsString()
        {
            double secElasped = (DateTime.UtcNow - StartDateTime).TotalSeconds;
            double minElasped = (DateTime.UtcNow - StartDateTime).TotalMinutes;
            double hourElasped = minElasped > 60 ? 0 : minElasped / 60;

            int sec = 0;
            int min = 0;
            int hour = 0;
            string minStr = "--";
            string hourStr = "--";
            if (Count > 0)
            {

                sec = (int)(Count / secElasped);
                min = minElasped >= 1 ? (int)(Count / minElasped) : 0;
                hour = hourElasped >= 1 ? (int)(Count / hourElasped) : 0;
                minStr = min == 0 ? "--" : min.ToString();
                hourStr = hour == 0 ? "--" : hour.ToString();
            }

            var tweets = string.Format("{0, 10}", "Tweets");
            var perhour = string.Format("{0, 7}", "Hour");
            var permin = string.Format("{0, 7}", "Min");
            var persec = string.Format("{0, 7}", "Sec");
            var emojis = string.Format("{0, 7}", "Emojis");
            var hashtags = string.Format("{0, 8}", "Hashtags");
            var urls = string.Format("{0, 7}", "URLs");
            var photos = string.Format("{0, 7}", "Photos");
            var percent = string.Format("{0, 7}", @"%");
            var emjper = WithEmojis * 100.0 / Count;
            var emjperstr = string.Format("{0, 7}", $"{emjper:###.##}");
            var hashper = WithHashTags * 100.0 / Count;
            var hashperstr = string.Format("{0, 7}", $"{hashper:###.##}");
            var urlper = WithUrls * 100.0 / Count;
            var urlperstr = string.Format("{0, 7}", $"{urlper:###.##}");
            var photoper = WithMedia * 100.0 / Count;
            var photoperstr = string.Format("{0, 7}", $"{photoper:###.##}");


            var builder = new StringBuilder();
            builder.AppendLine($"{tweets}{perhour}{permin}{persec}|{emojis}{percent}|{hashtags}{percent}|{urls}{percent}|{photos}{percent}");
            builder.AppendLine($"{Count,10}{hourStr,7}{minStr,7}{sec,7}|{WithEmojis,7}{emjperstr,7}|{WithHashTags,8}{hashperstr,7}|{WithUrls,7}{urlperstr,7}|{WithMedia,7}{photoperstr,7}");
            var statsString = builder.ToString();
            return statsString;
        }
    }
}
