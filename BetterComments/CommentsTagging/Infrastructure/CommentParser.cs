// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Abstract base class for all language-specific comment parsers.
    ///
    /// Flow:
    ///   1. <see cref="GetCommentType"/> strips the comment delimiter, trims leading whitespace,
    ///      and delegates to <see cref="TokenMatcher.Match"/>.
    ///   2. If the type is <see cref="CommentType.Normal"/> the span is returned as-is.
    ///   3. If "highlight keyword only" is enabled for the matched type, only the token span is
    ///      returned (e.g. just "TODO").
    ///   4. Otherwise <see cref="SpecificParse"/> is called for language-aware span construction
    ///      (handles multi-line blocks, partial spans, etc.).
    /// </summary>
    internal abstract class CommentParser : ICommentParser
    {
        /// <summary>Shared case-insensitive comparison constant.</summary>
        protected static readonly System.StringComparison OrdinalIgnoreCase =
            System.StringComparison.OrdinalIgnoreCase;

        /// <summary>Extension settings used to query per-classification options at parse time.</summary>
        protected readonly BetterCommentsSettings Settings;

        /// <summary>Initialises the parser with the active settings instance.</summary>
        protected CommentParser(BetterCommentsSettings settings) => Settings = settings;

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  ICommentParser
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public abstract bool IsValidComment(SnapshotSpan span);

        /// <inheritdoc/>
        public virtual Comment Parse(SnapshotSpan span)
        {
            var raw = span.GetText();
            var delLen = GetDelimiterLength(span);
            var content = delLen < raw.Length ? raw.Substring(delLen) : string.Empty;
            var trimmedContent = content.TrimStart();
            var leadingSpace = content.Length - trimmedContent.Length;

            var matchResult = TokenMatcher.Match(trimmedContent);

            if (matchResult.Type == CommentType.Normal)
                return new Comment(span, CommentType.Normal);

            // ── Keyword-only highlight ────────────────────────────────────────────────────────
            if (ShouldHighlightKeywordOnly(matchResult.Type))
            {
                var tokStart = delLen + leadingSpace + matchResult.Index;
                return new Comment(
                    new SnapshotSpan(span.Snapshot, span.Start + tokStart, matchResult.Token.Length),
                    matchResult.Type);
            }

            return SpecificParse(span, matchResult.Type);
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Abstract / virtual members for subclasses
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Determines the comment type by stripping the leading delimiter and passing the
        /// remaining content (trimmed) to <see cref="TokenMatcher.Match"/>.
        /// </summary>
        protected virtual CommentType GetCommentType(SnapshotSpan span)
        {
            var raw     = span.GetText();
            var delLen  = GetDelimiterLength(span);
            var content = delLen < raw.Length ? raw.Substring(delLen).TrimStart() : string.Empty;
            return TokenMatcher.Match(content).Type;
        }

        /// <summary>
        /// Returns the character length of the opening comment delimiter for this span
        /// (e.g. 2 for <c>//</c> or <c>/*</c>, 1 for <c>#</c>, 4 for <c>&lt;!--</c>).
        /// </summary>
        protected abstract int GetDelimiterLength(SnapshotSpan span);

        /// <summary>
        /// Produces the final tagged <see cref="Comment"/> when the caller has already determined
        /// that the span is not <see cref="CommentType.Normal"/> and keyword-only mode is inactive.
        /// Responsible for computing accurate sub-span positions (handles multi-line blocks, etc.).
        /// </summary>
        protected abstract Comment SpecificParse(SnapshotSpan span, CommentType commentType);

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Private helpers
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when only the matched keyword token should be highlighted
        /// (rather than the full comment content) for the given classification type.
        /// </summary>
        protected bool ShouldHighlightKeywordOnly(CommentType type)
        {
            switch (type)
            {
                case CommentType.Critical: return Settings.CriticalHighlightKeywordOnly;
                case CommentType.Warning:  return Settings.WarningHighlightKeywordOnly;
                case CommentType.Ideas:    return Settings.IdeasHighlightKeywordOnly;
                case CommentType.Info:     return Settings.InfoHighlightKeywordOnly;
                default:                   return false;
            }
        }
    }
}
