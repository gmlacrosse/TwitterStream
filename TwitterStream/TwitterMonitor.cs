using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Tweetinvi;

namespace TwitterStream
{
    public class TwitterMonitor
    {
        private static Dictionary<string, int> EmojisDic;
        private static Dictionary<string, int> HashTagsDic;
        private static Dictionary<string, int> UrlsDic;

        private static Counters Counters;

        public TwitterMonitor()
        {
            EmojisDic = new Dictionary<string, int>();
            HashTagsDic = new Dictionary<string, int>();
            UrlsDic = new Dictionary<string, int>();

            Counters = new Counters();
        }

        public void Start()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                UTF8Encoding uTF8 = new UTF8Encoding();
                Console.Write(Encoding.UTF8.GetString(uTF8.GetPreamble()));

                var apiKey = ConfigurationManager.AppSettings["APIKey"].ToString();
                var apiKeySecrete = ConfigurationManager.AppSettings["APISecretKey"].ToString();
                var accessToken = ConfigurationManager.AppSettings["AccessToken"].ToString();
                var accessTokenSecrete = ConfigurationManager.AppSettings["AccessTokenSecret"].ToString();

                Auth.SetUserCredentials(apiKey, apiKeySecrete, accessToken, accessTokenSecrete);

                var stream = Stream.CreateSampleStream();
                stream.TweetReceived += (sender, args) =>
                {
                    if (args.Tweet.Entities.Hashtags.Any())
                    {
                        Counters.WithHashTags++;

                        var hashtags = args.Tweet.Hashtags;

                        foreach (var hashtag in hashtags)
                        {
                            if (!HashTagsDic.ContainsKey(hashtag.Text))
                            {
                                HashTagsDic.Add(hashtag.Text, 1);
                            }
                            else
                            {
                                var c = HashTagsDic[hashtag.Text];
                                c++;
                                HashTagsDic[hashtag.Text] = c;
                            }
                        }
                    }

                    //  sample tweets with emojis
                    // "@BTS_twt JOONI JJANG💜💜💜"
                    // "Good morning 😁"

                    bool hasEmojis = false;

                    List<string> emlist = new List<string>();

                    emlist = EmojiParser.GetEmojisNames(args.Tweet.Text);

                    if (emlist.Any())
                    {
                        Counters.WithEmojis++;

                        foreach (var emj in emlist)
                        {
                            if (!EmojisDic.ContainsKey(emj))
                            {
                                EmojisDic.Add(emj, 1);
                            }
                            else
                            {
                                var c = EmojisDic[emj];
                                c++;
                                EmojisDic[emj] = c;
                            }
                        }
                    }

                    if (hasEmojis) Counters.WithEmojis++;

                    List<string> urlsList = new List<string>();

                    bool hasUrls = false;
                    if (args.Tweet.Entities.Urls.Any())
                    {
                        hasUrls = true;

                        foreach (var ent in args.Tweet.Entities.Urls)
                            urlsList.Add(ent.URL);
                    }

                    if (hasUrls) Counters.WithUrls++;

                    if (args.Tweet.Media.Any())
                    {
                        Counters.WithMedia++;

                        foreach (var m in args.Tweet.Media)
                            urlsList.Add("http://" + m.DisplayURL);
                    }

                    var extractedURLs = Extractor.ExtractURLs(args.Tweet.Text);

                    if (extractedURLs.Any())
                        urlsList.AddRange(extractedURLs);

                    foreach (var u in urlsList)
                    {
                        var uri = new Uri(u);
                        var domain = uri.Host;

                        if (!UrlsDic.ContainsKey(domain))
                        {
                            UrlsDic.Add(domain, 1);
                        }
                        else
                        {
                            var c = UrlsDic[domain];
                            c++;
                            UrlsDic[domain] = c;
                        }
                    }

                    Counters.Count += 1;
                    if (Counters.Count % 100 == 0)
                    {
                        DisplayResult();
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

        private static void DisplayResult()
        {
            Console.WriteLine(Counters.GetStatsAsString());

            if (EmojisDic.Count > 0)
            {
                string topstr = string.Format("{0, 25}", "Top Emojis");
                Console.WriteLine($"\r\n{topstr}");
                var top5 = Extractor.GetTopFromDictionary(EmojisDic, 5);
                foreach (var rec in top5)
                    Console.WriteLine($"{rec.Item1,30}");
            }

            if (HashTagsDic.Count > 0)
            {
                string topstr = string.Format("{0, 25}", "Top Hashtags");
                Console.WriteLine($"\r\n{topstr}");
                var top5 = Extractor.GetTopFromDictionary(HashTagsDic, 5);
                foreach (var rec in top5)
                    Console.WriteLine($"{rec.Item1,30}");
            }

            if (UrlsDic.Count > 0)
            {
                string topstr = string.Format("{0, 25}", "Top Domains");
                Console.WriteLine($"\r\n{topstr}");
                var top5 = Extractor.GetTopFromDictionary(UrlsDic, 5);
                foreach (var rec in top5)
                    Console.WriteLine($"{rec.Item1,30}");
            }

            Console.WriteLine("\r\nPress Ctrl-C to exit.\r\n");
        }
    }
}
