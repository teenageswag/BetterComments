// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Parses C/C++ single-line (<c>//</c>) and block (<c>/* … */</c>) comments.
    /// Multi-line block comments use the same per-section colouring as the C# parser.
    /// </summary>
    internal sealed class CppCommentParser : CommentParser
    {
        private const string Opener = "/*";
        private const string Closer = "*/";

        /// <summary>Initialises the parser with the active settings instance.</summary>
        public CppCommentParser(BetterCommentsSettings settings) : base(settings) { }

        /// <inheritdoc/>
        public override bool IsValidComment(SnapshotSpan span)
        {
            // If the Visual Studio tagger classified it as a comment, it is valid.
            return true;
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => 2;

        /// <inheritdoc/>
        public override Comment Parse(SnapshotSpan span)
        {
            var txt = span.GetText();
            if (txt.StartsWith("//", System.StringComparison.OrdinalIgnoreCase))
            {
                // Single-line "//" uses the standard base-class flow (type detect → SpecificParse).
                return base.Parse(span);
            }

            // It's a block comment (or a continuation line of one).
            // Visual Studio sometimes yields line-by-line classification spans for block comments.
            // We expand the span to the full /* ... */ block to process it contextually.
            var fullSpan = ParseHelper.ExpandToFullBlockComment(span, Opener, Closer);
            return new Comment(ParseHelper.ParseBlockCommentSegments(
                fullSpan, Opener, Closer, ShouldHighlightKeywordOnly));
        }

        /// <inheritdoc/>
        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType, string customTagId)
        {
            var spanText   = span.GetText();
            var tokenStart = ParseHelper.FindTokenStart(spanText, GetDelimiterLength(span));

            if (tokenStart < 0)
                return new Comment(span, CommentType.Normal);

            return new Comment(
                new SnapshotSpan(span.Snapshot, span.Start + tokenStart, span.Length - tokenStart),
                commentType,
                customTagId);
        }
    }
}
