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
            var txt = span.GetText();
            return txt.StartsWith("//", OrdinalIgnoreCase)
                || txt.StartsWith(Opener, OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => 2;

        /// <inheritdoc/>
        public override Comment Parse(SnapshotSpan span)
        {
            if (span.GetText().StartsWith(Opener, OrdinalIgnoreCase))
                return new Comment(ParseHelper.ParseBlockCommentSegments(
                    span, Opener, Closer, ShouldHighlightKeywordOnly));

            return base.Parse(span);
        }

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
    }
}
