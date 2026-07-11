// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Parses Visual Basic single-line comments that start with <c>'</c>.
    /// </summary>
    internal sealed class VBCommentParser : CommentParser
    {
        /// <summary>Initialises the parser with the active settings instance.</summary>
        public VBCommentParser(BetterCommentsSettings settings) : base(settings) { }

        /// <inheritdoc/>
        public override bool IsValidComment(SnapshotSpan span)
            => span.GetText().TrimStart().StartsWith("'", OrdinalIgnoreCase);

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => 1; // "'"

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
