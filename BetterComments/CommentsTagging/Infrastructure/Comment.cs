// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// A single classified region inside a comment: one <see cref="SnapshotSpan"/> paired with
    /// the <see cref="CommentType"/> that should be applied to it.
    /// </summary>
    internal readonly struct CommentSegment
    {
        /// <summary>The text region to tag.</summary>
        public readonly SnapshotSpan Span;

        /// <summary>The Better Comments classification for this region.</summary>
        public readonly CommentType Type;

        /// <summary>Initialises a new <see cref="CommentSegment"/>.</summary>
        public CommentSegment(SnapshotSpan span, CommentType type)
        {
            Span = span;
            Type = type;
        }
    }

    /// <summary>
    /// Represents the result of parsing one comment span.
    ///
    /// A single <see cref="Comment"/> may carry <em>multiple</em>
    /// <see cref="CommentSegment"/> objects with <em>different</em> types — this is used for
    /// multi-line block comments where different sections carry different tokens
    /// (e.g. <c>/* err: …\n todo: … */</c>).
    /// </summary>
    internal sealed class Comment
    {
        /// <summary>
        /// All classified segments produced from the source span.
        /// Segments with <see cref="CommentType.Normal"/> are included so callers can inspect
        /// them, but the tagger filters them out before emitting tags.
        /// </summary>
        public IReadOnlyList<CommentSegment> Segments { get; }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Constructors
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="Comment"/> from a heterogeneous list of pre-built segments
        /// (used by the multi-line block-comment parser).
        /// </summary>
        public Comment(IEnumerable<CommentSegment> segments)
        {
            Segments = new List<CommentSegment>(segments);
        }

        /// <summary>
        /// Creates a <see cref="Comment"/> where every span shares the same classification
        /// (used by all single-line parsers).
        /// </summary>
        public Comment(IEnumerable<SnapshotSpan> spans, CommentType type)
        {
            Segments = spans.Select(s => new CommentSegment(s, type)).ToList();
        }

        /// <summary>
        /// Creates a <see cref="Comment"/> from a single span (convenience overload).
        /// </summary>
        public Comment(SnapshotSpan span, CommentType type)
        {
            Segments = new List<CommentSegment> { new CommentSegment(span, type) };
        }
    }
}
