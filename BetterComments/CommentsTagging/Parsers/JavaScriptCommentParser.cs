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
            var fullSpan = ParseHelper.ExpandToFullBlockComment(span, "/*", "*/");
            return new Comment(ParseHelper.ParseBlockCommentSegments(
                fullSpan, "/*", "*/", ShouldHighlightKeywordOnly));
        }

        /// <inheritdoc/>
        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType, string customTagId)
        {
            // Only reached for "//" single-line comments.
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
