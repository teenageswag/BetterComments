using System.Text.RegularExpressions;

namespace BetterComments.CommentsTagging
{
    internal readonly struct TokenMatchResult
    {
        public static readonly TokenMatchResult Normal = new TokenMatchResult(CommentType.Normal, -1, null);

        public CommentType Type { get; }
        public int Index { get; }
        public string Token { get; }

        public TokenMatchResult(CommentType type, int index, string token)
        {
            Type = type;
            Index = index;
            Token = token;
        }
    }

    internal static class TokenMatcher
    {
        // Replicating logic from example_src (TS) tagParser.ts
        // Pattern: (^|[^\w])(ERROR|ERR|FIX|FIXME|WARNING|WARN|TODO|IDEA|OPTIMIZE|NOTE|INFO)\s*(\([^\r\n)]*\))?\s*:
        private static readonly Regex TagRegex = new Regex(
            @"(^|[^\w])(ERROR|ERR|FIX|FIXME|WARNING|WARN|TODO|IDEA|OPTIMIZE|NOTE|INFO)\s*(\([^\r\n)]*\))?\s*:",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static TokenMatchResult Match(string text)
        {
            if (string.IsNullOrEmpty(text))
                return TokenMatchResult.Normal;

            var match = TagRegex.Match(text);
            if (!match.Success)
            {
                return TokenMatchResult.Normal;
            }

            string tag = match.Groups[2].Value.ToUpperInvariant();
            CommentType type;
            switch (tag)
            {
                case "ERROR": case "ERR": case "FIX": case "FIXME":
                    type = CommentType.Critical;
                    break;
                case "WARNING": case "WARN":
                    type = CommentType.Warning;
                    break;
                case "TODO": case "IDEA": case "OPTIMIZE":
                    type = CommentType.Ideas;
                    break;
                case "NOTE": case "INFO":
                    type = CommentType.Info;
                    break;
                default:
                    return TokenMatchResult.Normal;
            }

            int index = match.Index + match.Groups[1].Length;
            string token = text.Substring(index, match.Length - (index - match.Index));
            return new TokenMatchResult(type, index, token);
        }
    }
}
