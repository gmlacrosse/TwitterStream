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
            int hour= 0;
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
            var statsString = $"Stats\r\n----------------------------------------\r\nCount : {Count}\r\nHour : {hourStr}\r\nMin : {minStr}\r\nSec : {sec}";
            return statsString;
        }
    }
}
