using NeoSmart.Unicode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Core.Helpers;

namespace TwitterStream
{
    public class TwitterMonitor
    {
        private List<Emojis> Emojis;
        private static Dictionary<string, int> EmojisDic;
        private static Dictionary<string, int> HashTagsDic;
        private static Dictionary<string, int> UrlsDic;

        private static HashSet<string> EmojiTexts;
        private static HashSet<string> Unified;

        private static Counters Counters;

        public TwitterMonitor()
        {
            EmojisDic = new Dictionary<string, int>();
            HashTagsDic = new Dictionary<string, int>();
            UrlsDic = new Dictionary<string, int>();

            EmojiTexts = new HashSet<string>();
            Unified = new HashSet<string>();
            Counters = new Counters();
        }

        public void Start()
        {
            try
            {
                string json = File.ReadAllText("emojis.json");
                Emojis = JsonConvert.DeserializeObject<List<Emojis>>(json);


                foreach (var e in Emojis)
                {
                    EmojiTexts.Add(e.short_name.ToLower());

                    Unified.Add(e.unified);
                }

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                var apiKey = ConfigurationManager.AppSettings["APIKey"].ToString();
                var apiKeySecrete = ConfigurationManager.AppSettings["APISecretKey"].ToString();
                var accessToken = ConfigurationManager.AppSettings["AccessToken"].ToString();
                var accessTokenSecrete = ConfigurationManager.AppSettings["AccessTokenSecret"].ToString();

                Auth.SetUserCredentials(apiKey, apiKeySecrete, accessToken, accessTokenSecrete);

                var stream = Tweetinvi.Stream.CreateSampleStream();
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

                    var emlist = Extractor.ExtractEmojis(args.Tweet.Text);

                    if (emlist.Any())
                    {
                        foreach (var e in emlist)
                        {
                            hasEmojis = true;
                            var bytes = Encoding.UTF8.GetBytes(e);
                            var unicode = Encoding.Unicode.GetString(bytes);

                            if (!EmojisDic.ContainsKey(unicode))
                            {
                                EmojisDic.Add(unicode, 1);
                            }
                            else
                            {
                                var c = EmojisDic[unicode];
                                c++;
                                EmojisDic[unicode] = c;
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
                    }

                    foreach (var u in urlsList)
                    {
                        if (!UrlsDic.ContainsKey(u))
                        {
                            UrlsDic.Add(u, 1);
                        }
                        else
                        {
                            var c = UrlsDic[u];
                            c++;
                            UrlsDic[u] = c;
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
            var top5str = string.Format("{0, 25}", "Top 5");

            if (EmojisDic.Count > 0)
            {
                Console.WriteLine($"\r\n{top5str} Emojis");
                var top5 = Extractor.GetTopFromDictionary(EmojisDic, 5);
                foreach (var rec in top5)
                    Console.WriteLine($"{rec.Item1,30}");
            }

            if (HashTagsDic.Count > 0)
            {
                Console.WriteLine($"\r\n{top5str} Hashtags");
                var top5 = Extractor.GetTopFromDictionary(HashTagsDic, 5);
                foreach (var rec in top5)
                    Console.WriteLine($"{rec.Item1,35}");
            }

            if (UrlsDic.Count > 0)
            {
                Console.WriteLine($"\r\n{top5str} URLs");
                var top5 = Extractor.GetTopFromDictionary(UrlsDic, 5);
                foreach (var rec in top5)
                    Console.WriteLine($"{rec.Item1,45}");
            }

            Console.WriteLine("\r\nPress Ctrl-C to exit.\r\n");
        }
    }
}
