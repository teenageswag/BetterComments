// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Contract for language-specific comment parsers.  Each parser receives a
    /// <see cref="SnapshotSpan"/> that the underlying classification tagger has already identified
    /// as a comment and must determine its <see cref="CommentType"/> and the exact sub-spans to tag.
    /// </summary>
    internal interface ICommentParser
    {
        /// <summary>
        /// Returns <c>true</c> when <paramref name="span"/> represents a comment that this parser
        /// can handle (e.g. starts with the correct delimiter such as <c>//</c> or <c>/*</c>).
        /// </summary>
        bool IsValidComment(SnapshotSpan span);

        /// <summary>
        /// Parses <paramref name="span"/> and returns a <see cref="Comment"/> that describes
        /// which sub-spans should be classified and with which <see cref="CommentType"/>.
        /// </summary>
        Comment Parse(SnapshotSpan span);
    }
}
