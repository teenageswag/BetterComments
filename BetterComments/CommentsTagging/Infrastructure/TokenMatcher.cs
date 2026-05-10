using System.Text.RegularExpressions;

namespace BetterComments.CommentsTagging
{
    internal static class TokenMatcher
    {
        // Replicating logic from example_src (TS) tagParser.ts
        // Pattern: (^|[^\w])(ERROR|ERR|FIX|FIXME|WARNING|WARN|TODO|IDEA|OPTIMIZE|NOTE|INFO)\s*(\([^\r\n)]*\))?\s*:
        private static readonly Regex TagRegex = new Regex(
            @"(^|[^\w])(ERROR|ERR|FIX|FIXME|WARNING|WARN|TODO|IDEA|OPTIMIZE|NOTE|INFO)\s*(\([^\r\n)]*\))?\s*:",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static CommentType Match(string text)
        {
            var match = TagRegex.Match(text);
            if (!match.Success)
            {
                return CommentType.Normal;
            }

            string tag = match.Groups[2].Value.ToUpperInvariant();
            switch (tag)
            {
                case "ERROR": case "ERR": case "FIX": case "FIXME":
                    return CommentType.Critical;
                case "WARNING": case "WARN":
                    return CommentType.Warning;
                case "TODO": case "IDEA": case "OPTIMIZE":
                    return CommentType.Ideas;
                case "NOTE": case "INFO":
                    return CommentType.Info;
                default:
                    return CommentType.Normal;
            }
        }

        // Returns the match starting index
        public static int FindTokenIndex(string text)
        {
            var match = TagRegex.Match(text);
            if (!match.Success) return -1;
            
            // match.Groups[1] is the (^|[^\w]) part
            return match.Index + match.Groups[1].Length;
        }

        // Returns the matched part, including the trailing colon and optional params.
        public static string GetMatchedToken(string text)
        {
            var match = TagRegex.Match(text);
            if (!match.Success) return null;
            
            // We return the actual tag part: (TAG)(\(param\))?:
            // This is matched by Groups[2], Groups[3], etc., but we can just substring
            // from FindTokenIndex
            int start = FindTokenIndex(text);
            return text.Substring(start, match.Length - (start - match.Index));
        }
    }
}
