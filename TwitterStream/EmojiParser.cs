using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TwitterStream
{
    public static class EmojiParser
    {
        static readonly Dictionary<string, string> _emojis;
        static readonly Regex _emojisRegex;
        static EmojiParser()
        {
            // load mentioned json from somewhere
            var data = JArray.Parse(File.ReadAllText("emojis.json"));
            _emojis = data.OfType<JObject>().ToDictionary(
                c =>
                {
                    var unicodeRaw = ((JValue)c["unified"]).Value.ToString();
                    var chars = new List<char>();
                    // some characters are multibyte in UTF32, split them
                    foreach (var point in unicodeRaw.Split('-'))
                    {
                        // parse hex to 32-bit unsigned integer (UTF32)
                        uint unicodeInt = uint.Parse(point, System.Globalization.NumberStyles.HexNumber);
                        // convert to bytes and get chars with UTF32 encoding
                        chars.AddRange(Encoding.UTF32.GetChars(BitConverter.GetBytes(unicodeInt)));
                    }
                    // this is resulting emoji
                    return new string(chars.ToArray());
                },
                c => ((JValue)c["short_name"]).Value.ToString());

            // build huge regex 
            _emojisRegex = new Regex(String.Join("|", _emojis.Keys.Select(Regex.Escape)));
        }

        public static List<string> GetEmojisNames(string input)
        {
            List<string> names = new List<string>();
            var matches = _emojisRegex.Matches(input);
            foreach (var match in matches)
            {
                if (_emojis.ContainsKey(match.ToString()))
                {
                    names.Add(_emojis[match.ToString()]);
                }
                else
                {
                    names.Add(match.ToString());
                }
            }

            return names;
        }
    }
}
