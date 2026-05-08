// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Parses JavaScript and TypeScript single-line (<c>//</c>) and single-line delimited
    /// (<c>/* … */</c>) comments.
    ///
    /// The VS JavaScript/TypeScript tagger sometimes delivers only a partial span (e.g. just the
    /// opening token characters) rather than the complete comment line.  This parser uses
    /// <see cref="ParseHelper.CompleteSingleLineCommentSpan"/> and
    /// <see cref="ParseHelper.CompleteDelimitedCommentSpan"/> to reconstruct the full span from
    /// the underlying snapshot before performing token detection.
    /// </summary>
    internal sealed class JavaScriptCommentParser : CommentParser
    {
        /// <summary>Initialises the parser with the active settings instance.</summary>
        public JavaScriptCommentParser(BetterCommentsSettings settings) : base(settings) { }

        /// <inheritdoc/>
        public override bool IsValidComment(SnapshotSpan span)
        {
            var txt = span.GetText();
            return txt.StartsWith("//", OrdinalIgnoreCase)
                || txt.StartsWith("/*", OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => 2;

        /// <inheritdoc/>
        protected override CommentType GetCommentType(SnapshotSpan span)
        {
            // Reconstruct the full span first so token detection works on the complete text.
            SnapshotSpan full;
            var txt = span.GetText();

            if (txt.Contains("//"))
            {
                full = ParseHelper.CompleteSingleLineCommentSpan(span, "//");
            }
            else
            {
                var parts = ParseHelper.CompleteDelimitedCommentSpan(span, "/*", "*/");
                if (parts.Count == 0) return CommentType.Normal;
                full = parts[0];
            }

            var content = full.GetText().Substring(GetDelimiterLength(full)).TrimStart();
            return TokenMatcher.Match(content);
        }

        /// <inheritdoc/>
        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
        {
            var txt = span.GetText();

            if (txt.Contains("//"))
            {
                var full       = ParseHelper.CompleteSingleLineCommentSpan(span, "//");
                var fullText   = full.GetText();
                var delimLen   = GetDelimiterLength(full);
                var tokenStart = ParseHelper.FindTokenStart(fullText, delimLen);

                if (tokenStart >= 0)
                {
                    var spanLength = fullText.Length - tokenStart;
                    if (spanLength > 0)
                        return new Comment(
                            new SnapshotSpan(full.Snapshot, full.Start + tokenStart, spanLength),
                            commentType);
                }
            }
            else if (txt.Contains("/*"))
            {
                var parts = ParseHelper.CompleteDelimitedCommentSpan(span, "/*", "*/");
                if (parts.Count > 0)
                {
                    var full       = parts[0];
                    var fullText   = full.GetText();
                    var delimLen   = GetDelimiterLength(full);
                    var tokenStart = ParseHelper.FindTokenStart(fullText, delimLen);

                    if (tokenStart >= 0)
                    {
                        var closerIdx  = fullText.IndexOf("*/", OrdinalIgnoreCase);
                        var lastChar   = fullText.IndexOfFirstCharReverse(closerIdx - 1);
                        var spanLength = lastChar >= tokenStart ? lastChar - tokenStart + 1 : 0;

                        if (spanLength > 0)
                            return new Comment(
                                new SnapshotSpan(full.Snapshot, full.Start + tokenStart, spanLength),
                                commentType);
                    }
                }
            }

            return new Comment(new List<SnapshotSpan>(), commentType);
        }
    }
}
