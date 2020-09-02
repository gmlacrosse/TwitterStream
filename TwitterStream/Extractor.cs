using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TwitterStream
{
    public class Extractor
    {
        public static List<string> ExtractURLs(string text)
        {
            var urls = new List<string>();

            var rx = new Regex(@"http(s) ?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var matches = rx.Matches(text);

            foreach (var match in matches)
            {
                urls.Add(match.ToString());
            }

            return urls;
        }

        public static List<Tuple<string, int>> GetTopFromDictionary(IDictionary<string, int> dict, int top)
        {
            var result = new List<Tuple<string, int>>();

            var ordered = dict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Take(top);

            foreach (var rec in ordered)
                result.Add(new Tuple<string, int>(rec.Key, rec.Value));

            return result;
        }
    }
}
