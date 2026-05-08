// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Parses single-line HTML and XAML comments of the form <c>&lt;!-- token … --&gt;</c>.
    /// Multi-line markup comments are not supported (the VS XAML/HTML taggers split them across
    /// individual spans).
    /// </summary>
    internal sealed class MarkupCommentParser : CommentParser
    {
        private const string Opener = "<!--";
        private const string Closer = "-->";

        /// <summary>Initialises the parser with the active settings instance.</summary>
        public MarkupCommentParser(BetterCommentsSettings settings) : base(settings) { }

        /// <inheritdoc/>
        public override bool IsValidComment(SnapshotSpan span)
        {
            var txt = span.GetText();
            return !txt.Contains("\r\n")
                && txt.Contains(Opener)
                && txt.Contains(Closer);
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => Opener.Length; // 4

        /// <inheritdoc/>
        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
        {
            var spanText   = span.GetText();
            var delimLen   = GetDelimiterLength(span);
            var tokenStart = ParseHelper.FindTokenStart(spanText, delimLen);

            if (tokenStart < 0)
                return new Comment(span, CommentType.Normal);

            var closerIdx  = spanText.IndexOf(Closer, OrdinalIgnoreCase);
            var lastChar   = spanText.IndexOfFirstCharReverse(closerIdx - 1);
            var spanLength = lastChar >= tokenStart ? lastChar - tokenStart + 1 : 0;

            if (spanLength <= 0)
                return new Comment(span, CommentType.Normal);

            return new Comment(
                new SnapshotSpan(span.Snapshot, span.Start + tokenStart, spanLength),
                commentType);
        }
    }
}
