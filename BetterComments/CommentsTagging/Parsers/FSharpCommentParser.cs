// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Parses F# single-line (<c>//</c>) and block (<c>(* … *)</c>) comments.
    /// Multi-line block comments use the same per-section colouring as the C# parser.
    /// </summary>
    internal sealed class FSharpCommentParser : CommentParser
    {
        private const string BlockOpener = "(*";
        private const string BlockCloser = "*)";

        /// <summary>Initialises the parser with the active settings instance.</summary>
        public FSharpCommentParser(BetterCommentsSettings settings) : base(settings) { }

        /// <inheritdoc/>
        public override bool IsValidComment(SnapshotSpan span)
        {
            var txt = span.GetText();
            return txt.StartsWith("//", OrdinalIgnoreCase)
                || txt.StartsWith(BlockOpener, OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => 2;

        /// <inheritdoc/>
        public override Comment Parse(SnapshotSpan span)
        {
            if (span.GetText().StartsWith(BlockOpener, OrdinalIgnoreCase))
                return new Comment(ParseHelper.ParseBlockCommentSegments(
                    span, BlockOpener, BlockCloser, ShouldHighlightKeywordOnly));

            return base.Parse(span);
        }

        /// <inheritdoc/>
        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
        {
            // Only reached for "//" single-line comments.
            var spanText   = span.GetText();
            var tokenStart = ParseHelper.FindTokenStart(spanText, GetDelimiterLength(span));

            if (tokenStart < 0)
                return new Comment(span, CommentType.Normal);

            return new Comment(
                new SnapshotSpan(span.Snapshot, span.Start + tokenStart, span.Length - tokenStart),
                commentType);
        }
    }
}
