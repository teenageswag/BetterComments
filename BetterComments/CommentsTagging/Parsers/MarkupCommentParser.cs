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
            // If the Visual Studio tagger classified it as a comment, it is valid.
            return true;
        }

        /// <inheritdoc/>
        protected override int GetDelimiterLength(SnapshotSpan span) => Opener.Length; // 4

        /// <inheritdoc/>
        public override Comment Parse(SnapshotSpan span)
        {
            // Markup doesn't have // style comments, only <!-- -->
            var fullSpan = ParseHelper.ExpandToFullBlockComment(span, Opener, Closer);
            return new Comment(ParseHelper.ParseBlockCommentSegments(
                fullSpan, Opener, Closer, ShouldHighlightKeywordOnly));
        }

        /// <inheritdoc/>
        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
        {
            // This is only used for the base Parse logic, which we override.
            return new Comment(span, CommentType.Normal);
        }
    }
}
