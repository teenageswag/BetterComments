using BetterComments.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BetterComments.CommentsTagging
{
    internal readonly struct TokenMatchResult
    {
        public static readonly TokenMatchResult Normal = new TokenMatchResult(CommentType.Normal, -1, null, null);

        public CommentType Type { get; }
        public int Index { get; }
        public string Token { get; }
        public string CustomTagId { get; }

        public bool IsCustomTag => CustomTagId != null;

        public TokenMatchResult(CommentType type, int index, string token, string customTagId)
        {
            Type = type;
            Index = index;
            Token = token;
            CustomTagId = customTagId;
        }
    }

    internal static class TokenMatcher
    {
        // Built-in tags regex
        private static readonly Regex BuiltInTagRegex = new Regex(
            @"(^|[^\w])(ERROR|ERR|FIX|FIXME|WARNING|WARN|TODO|IDEA|OPTIMIZE|NOTE|INFO)\s*(\([^\r\n)]*\))?\s*:",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Custom tags regex (built dynamically)
        private static Regex customTagRegex;
        private static readonly object regexLock = new object();
        private sealed class MatchTag
        {
            public string Tag { get; set; }
            public CommentType MappedType { get; set; }
        }

        private static MatchTag[] currentCustomTags = new MatchTag[0];

        /// <summary>
        /// Updates the custom tags used for matching.
        /// Rebuilds the regex pattern when custom tags change.
        /// </summary>
        public static void UpdateCustomTags(IEnumerable<CustomTagDefinition> customTags)
        {
            var tags = customTags?.Where(t => t != null && t.Enabled && !string.IsNullOrWhiteSpace(t.Tag))
                .Select(t => new MatchTag { Tag = t.Tag, MappedType = t.MappedType })
                .ToArray() ?? new MatchTag[0];

            lock (regexLock)
            {
                if (tags.Length == currentCustomTags.Length
                    && tags.Zip(currentCustomTags, (left, right) =>
                        string.Equals(left.Tag, right.Tag, StringComparison.OrdinalIgnoreCase)
                        && left.MappedType == right.MappedType).All(equal => equal))
                    return;

                currentCustomTags = tags;

                if (tags.Length > 0)
                {
                    var tagAlternation = string.Join("|", tags.OrderByDescending(t => t.Tag.Length).Select(t => Regex.Escape(t.Tag)));
                    var pattern = $@"(^|[^\w])({tagAlternation})\s*(\([^\r\n)]*\))?\s*:";
                    customTagRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                else
                {
                    customTagRegex = null;
                }
            }
        }

        public static TokenMatchResult Match(string text)
        {
            if (string.IsNullOrEmpty(text))
                return TokenMatchResult.Normal;

            // Try built-in tags first
            var builtInResult = MatchBuiltInTags(text);
            if (builtInResult.Type != CommentType.Normal)
                return builtInResult;

            // Try custom tags
            var customResult = MatchCustomTags(text);
            if (customResult.Type != CommentType.Normal)
                return customResult;

            return TokenMatchResult.Normal;
        }

        private static TokenMatchResult MatchBuiltInTags(string text)
        {
            var match = BuiltInTagRegex.Match(text);
            if (!match.Success)
                return TokenMatchResult.Normal;

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
            return new TokenMatchResult(type, index, token, null);
        }

        private static TokenMatchResult MatchCustomTags(string text)
        {
            Regex regex;
            MatchTag[] tags;

            lock (regexLock)
            {
                regex = customTagRegex;
                tags = currentCustomTags;
            }

            if (regex == null || tags.Length == 0)
                return TokenMatchResult.Normal;

            var match = regex.Match(text);
            if (!match.Success)
                return TokenMatchResult.Normal;

            string matchedTag = match.Groups[2].Value.ToUpperInvariant();
            var customTag = tags.FirstOrDefault(t =>
                string.Equals(t.Tag, matchedTag, StringComparison.OrdinalIgnoreCase));

            if (customTag == null)
                return TokenMatchResult.Normal;

            int index = match.Index + match.Groups[1].Length;
            string token = text.Substring(index, match.Length - (index - match.Index));
            return new TokenMatchResult(customTag.MappedType, index, token, matchedTag);
        }
    }
}
