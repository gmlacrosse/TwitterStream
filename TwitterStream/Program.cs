using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Tweetinvi;

namespace TwitterStream
{
    public class Program
    {
        public static List<Emojis> Emojis;

        public static HashSet<string> EmojiTexts;

        public static HashSet<string> Unified;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting TWitter Sample Stream Monitor Press Ctrl-C to exit.");
            Thread.Sleep(1000);

            string json = File.ReadAllText("emojis.json");
            Emojis = JsonConvert.DeserializeObject<List<Emojis>>(json);
            EmojiTexts = new HashSet<string>();
            Unified = new HashSet<string>();

            foreach (var e in Emojis)
            {
                EmojiTexts.Add(e.short_name.ToLower());
                Unified.Add(e.unified);
            }

            MonitorTWeets();
        }

        private static void MonitorTWeets()
        {
            try
            {
                var apiKey = ConfigurationManager.AppSettings["APIKey"].ToString();
                var apiKeySecrete = ConfigurationManager.AppSettings["APISecretKey"].ToString();
                var accessToken = ConfigurationManager.AppSettings["AccessToken"].ToString();
                var accessTokenSecrete = ConfigurationManager.AppSettings["AccessTokenSecret"].ToString();
                var counters = new Counters();
                var emojis = new Dictionary<string, int>();
                var urls = new Dictionary<string, int>();
                var hashTagsDic = new Dictionary<string, int>();
                var imageUrls = new Dictionary<string, int>();
                var symbolsDic = new Dictionary<string, int>();

                Auth.SetUserCredentials(apiKey, apiKeySecrete, accessToken, accessTokenSecrete);

                var stream = Tweetinvi.Stream.CreateSampleStream();
                stream.TweetReceived += (sender, args) =>
                {
                    var hashtags = args.Tweet.Hashtags;

                    foreach (var hashtag in hashtags)
                    {
                        if (!hashTagsDic.ContainsKey(hashtag.Text))
                        {
                            hashTagsDic.Add(hashtag.Text, 1);
                        }
                        else
                        {
                            var c = hashTagsDic[hashtag.Text];
                            hashTagsDic[hashtag.Text] = c += 1;
                        }
                    }

                    var text = args.Tweet.Text;
                    foreach (var e in Unified)
                    {
                        if (text.IndexOf(e, StringComparison.OrdinalIgnoreCase) >= 0)

                            if (!emojis.ContainsKey(e))
                            {
                                emojis.Add(e, 1);
                            }
                            else
                            {
                                var c = emojis[e];
                                emojis[e] = c += 1;
                            }
                    }

                    if (args.Tweet.Entities.Symbols.Any())
                    {
                        foreach(var e in args.Tweet.Entities.Symbols)
                        {
                            if (!symbolsDic.ContainsKey(e.Text))
                            {
                                symbolsDic.Add(e.Text, 1);
                            }
                            else
                            {
                                var c = symbolsDic[e.Text];
                                symbolsDic[e.Text] = c += 1;
                            }
                        }
                    }

                    List<string> urlsList = new List<string>();

                    if (args.Tweet.Entities.Urls.Any())
                        foreach (var ent in args.Tweet.Entities.Urls)
                            urlsList.Add(ent.URL);

                    foreach (var u in urlsList)
                    {
                        if (!urls.ContainsKey(u))
                        {
                            urls.Add(u, 1);
                        }
                        else
                        {
                            var c = urls[u];
                            urls[u] = c += 1;
                        }

                        if (u.ToLower().IndexOf("pic.twitter.com") >= 0 || u.ToLower().IndexOf("instagram.com") >= 0)
                        {
                            if (!imageUrls.ContainsKey(u))
                            {
                                imageUrls.Add(u, 1);
                            }
                            else
                            {
                                var c = imageUrls[u];
                                imageUrls[u] = c += 1;
                            }
                        }
                    }

                    var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    foreach (Match url in linkParser.Matches(text))
                    {
                        var u = url.ToString();
                        if (!urls.ContainsKey(u))
                        {
                            urls.Add(u, 1);
                        }
                        else
                        {
                            var c = urls[u];
                            urls[u] = c += 1;
                        }

                        if (u.ToLower().IndexOf("pic.twitter.com") >= 0 || u.ToLower().IndexOf("instagram.com") >= 0)
                        {
                            if (!imageUrls.ContainsKey(u))
                            {
                                imageUrls.Add(u, 1);
                            }
                            else
                            {
                                var c = imageUrls[u];
                                imageUrls[u] = c += 1;
                            }
                        }
                    }

                    counters.Count += 1;
                    if (counters.Count % 100 == 0)
                    {
                        Console.WriteLine(counters.GetStatsAsString());

                        var emjper = emojis.Count * 100.0 / counters.Count;

                        Console.WriteLine($"\r\nEmojis: {emojis.Count}| {emjper:0.##}%\r\n");

                        if (emojis.Count > 0)
                        {
                            Console.WriteLine("TOP 5\r\n");
                            var sorted = from entry in emojis orderby emojis.Values descending select entry;
                            int end = 0;
                            foreach (var ej in sorted)
                            {
                                end++;
                                Console.WriteLine($"{ej}");
                                if (end > 5) break;
                            }
                        }

                        var symper = symbolsDic.Count * 100.0 / counters.Count;

                        Console.WriteLine($"\r\nSymbols: {emojis.Count}| {symper:0.##}%\r\n");

                        if (symbolsDic.Count > 0)
                        {
                            Console.WriteLine("TOP 5\r\n");
                            var sorted = from entry in symbolsDic orderby symbolsDic.Values descending select entry;
                            int end = 0;
                            foreach (var symbol in sorted)
                            {
                                end++;
                                Console.WriteLine($"{symbol}");
                                if (end > 5) break;
                            }
                        }
                        var hashper = hashTagsDic.Count * 100.0 / counters.Count;

                        Console.WriteLine($"\r\nHashtags: {hashTagsDic.Count} | {hashper:0.##}%\r\n");

                        if (hashTagsDic.Count > 0)
                        {
                            Console.WriteLine("TOP 5\r\n");
                            var sorted = from entry in hashTagsDic orderby hashTagsDic.Values descending select entry;
                            int end = 0;
                            foreach (var ht in sorted)
                            {
                                end++;
                                Console.WriteLine($"{ht}");
                                if (end > 5) break;
                            }
                        }

                        var urlsper = urls.Count * 100.0 / counters.Count;

                        Console.WriteLine($"\r\nURLS: {urls.Count} | {urlsper:0.##}%\r\n");

                        if (urls.Count > 0)
                        {
                            Console.WriteLine("TOP 5\r\n");
                            var sorted = from entry in urls orderby urls.Values descending select entry;
                            int stop = 0;
                            foreach (var url in sorted)
                            {
                                stop++;
                                Console.WriteLine($"{url}");
                                if (stop > 5) break;
                            }
                        }

                        var iurlsper = imageUrls.Count * 100.0 / counters.Count;

                        Console.WriteLine($"\r\nImage URLS: {imageUrls.Count} | {iurlsper:0.##}% \r\n");

                        if (imageUrls.Count > 0)
                        {
                            Console.WriteLine("TOP 5\r\n");
                            var sorted = from entry in imageUrls orderby imageUrls.Values descending select entry;
                            int stop = 0;
                            foreach (var i in sorted)
                            {
                                stop++;
                                Console.WriteLine($"{i}");
                                if (stop > 5) break;
                            }
                        }
                        Console.WriteLine("\r\nPress Ctrl-C to exit.\r\n");
                    }
                };
                stream.StartStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
