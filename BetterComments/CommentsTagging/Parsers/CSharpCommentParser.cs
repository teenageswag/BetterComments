// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Parses C# single-line (<c>//</c>) and block (<c>/* … */</c>) comments.
    ///
    /// Single-line comments behave as before: the colour is applied from the token to the end
    /// of the line (or just the keyword in keyword-only mode).
    ///
    /// Block comments support <em>per-section</em> colouring:
    /// <code>
    /// /*
    ///  * err: this is red         ← Critical
    ///  * still red                ← Critical (continuation)
    ///  *                          ← empty → resets colour
    ///  * normal comment           ← uncoloured
    ///  * todo: this is blue       ← Ideas
    ///  * warn: now orange         ← Warning
    ///  * still orange             ← Warning (continuation)
    ///  */
    /// </code>
    /// Javadoc-style leading asterisks (<c> * content</c>) are stripped before token detection.
    /// </summary>
    internal sealed class CSharpCommentParser : CommentParser
    {
        private const string Opener = "/*";
        private const string Closer = "*/";

        /// <summary>Initialises the parser with the active settings instance.</summary>
        public CSharpCommentParser(BetterCommentsSettings settings) : base(settings) { }

        /// <inheritdoc/>
        public override bool IsValidComment(SnapshotSpan span)
        {
            var txt = span.GetText();
            return txt.StartsWith("//", OrdinalIgnoreCase)
                || txt.StartsWith(Opener, OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => 2;

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Parse override — block comments bypass the base type-detection flow
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public override Comment Parse(SnapshotSpan span)
        {
            // Block comments need full multi-section analysis — skip the base-class single-type
            // flow entirely.
            if (span.GetText().StartsWith(Opener, OrdinalIgnoreCase))
                return ParseBlockComment(span);

            // Single-line "//" uses the standard base-class flow (type detect → SpecificParse).
            return base.Parse(span);
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  SpecificParse — only reached for "//" single-line comments
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
        {
            var spanText   = span.GetText();
            var tokenStart = ParseHelper.FindTokenStart(spanText, GetDelimiterLength(span));

            if (tokenStart < 0)
                return new Comment(span, CommentType.Normal);

            return new Comment(
                new SnapshotSpan(span.Snapshot, span.Start + tokenStart, span.Length - tokenStart),
                commentType);
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
