// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Parses Go single-line (<c>//</c>) and block (<c>/* … */</c>) comments.
    /// Multi-line block comments use the same per-section colouring as the C# parser.
    /// </summary>
    internal sealed class GoCommentParser : CommentParser
    {
        private const string Opener = "/*";
        private const string Closer = "*/";

        /// <summary>Initialises the parser with the active settings instance.</summary>
        public GoCommentParser(BetterCommentsSettings settings) : base(settings) { }

        /// <inheritdoc/>
        public override bool IsValidComment(SnapshotSpan span)
        {
            // If the Visual Studio tagger classified it as a comment, it is valid.
            return true;
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => 2;

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Parse override — block comments bypass the base type-detection flow
        // ──────────────────────────────────────────────────────────────────────────────────────

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
            var fullSpan = ParseHelper.ExpandToFullBlockComment(span, Opener, Closer);
            return ParseBlockComment(fullSpan);
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  SpecificParse — only reached for "//" single-line comments
        // ──────────────────────────────────────────────────────────────────────────────────────

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

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Block comment parser
        // ──────────────────────────────────────────────────────────────────────────────────────

        private Comment ParseBlockComment(SnapshotSpan span)
        {
            var segments = ParseHelper.ParseBlockCommentSegments(
                span,
                Opener,
                Closer,
                ShouldHighlightKeywordOnly);

            return new Comment(segments);
        }
    }
}
