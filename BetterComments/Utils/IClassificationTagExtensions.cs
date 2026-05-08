// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text.Tagging;

namespace BetterComments
{
    /// <summary>
    /// Extension methods for <see cref="IClassificationTag"/> that help the tagger quickly
    /// determine whether a tag represents a comment or an XML documentation comment.
    /// </summary>
    internal static class IClassificationTagExtensions
    {
        /// <summary>
        /// Returns <c>true</c> when the tag's classification name contains the word "comment"
        /// (case-insensitive).
        /// </summary>
        public static bool IsComment(this IClassificationTag tag)
            => tag.ClassificationType.Classification.ContainsCaseIgnored("comment");

        /// <summary>
        /// Returns <c>true</c> when the tag's classification name contains "doc", indicating an
        /// XML documentation comment that should not be processed by Better Comments.
        /// </summary>
        public static bool IsXmlDoc(this IClassificationTag tag)
            => tag.ClassificationType.Classification.ContainsCaseIgnored("doc");
    }
}
