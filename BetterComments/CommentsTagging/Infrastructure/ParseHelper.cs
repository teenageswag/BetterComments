// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Stateless helper methods shared across language-specific comment parsers.
    /// All methods are pure with respect to VS shell state: no package, service, or settings access.
    /// Token detection is delegated to <see cref="TokenMatcher"/>.
    /// </summary>
    internal static class ParseHelper
    {
        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Single-line token location helpers
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the character offset (from the start of <paramref name="spanText"/>) at which
        /// the matched Better Comments token begins.
        /// Searches in the text that follows the comment delimiter.
        /// Returns <c>-1</c> if no token is found.
        /// </summary>
        /// <param name="spanText">Full text of the span — any casing.</param>
        /// <param name="delimLen">Character length of the leading comment delimiter (e.g. 2 for <c>//</c>).</param>
        public static int FindTokenStart(string spanText, int delimLen)
        {
            if (spanText == null || delimLen >= spanText.Length) return -1;

            var afterDelim = spanText.Substring(delimLen);
            var trimmed    = afterDelim.TrimStart();

            if (string.IsNullOrEmpty(trimmed))                     return -1;
            if (TokenMatcher.Match(trimmed) == CommentType.Normal) return -1;

            var leadingSpace = afterDelim.Length - trimmed.Length;
            return delimLen + leadingSpace;
        }

        /// <summary>
        /// Returns the matched token string found after the comment delimiter, or <c>null</c>.
        /// </summary>
        public static string GetMatchedToken(string spanText, int delimLen)
        {
            if (spanText == null || delimLen >= spanText.Length) return null;

            var afterDelim = spanText.Substring(delimLen);
            return TokenMatcher.GetMatchedToken(afterDelim.TrimStart());
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Multi-line block-comment parser
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a delimited block comment (e.g. <c>/* … */</c> or <c>(* … *)</c>) and returns
        /// a list of <see cref="CommentSegment"/> objects, one per classified region.
        ///
        /// Algorithm:
        /// <list type="bullet">
        ///   <item>A line that begins with a recognised token starts a new coloured section.</item>
        ///   <item>Subsequent non-empty lines with no token <em>inherit</em> the current section's colour.</item>
        ///   <item>An empty / whitespace-only line <em>resets</em> the current section to Normal.</item>
        ///   <item>Lines with no active section and no token are left uncoloured (Normal).</item>
        ///   <item>Leading <c>*</c> characters (Javadoc style: <c> * content</c>) are stripped before token detection but do not affect the coloured region start.</item>
        /// </list>
        /// </summary>
        /// <param name="span">The full snapshot span covering the block comment.</param>
        /// <param name="opener">Opening delimiter, e.g. <c>"/*"</c> or <c>"(*"</c>.</param>
        /// <param name="closer">Closing delimiter, e.g. <c>"*/"</c> or <c>"*)"</c>.</param>
        /// <param name="isKeywordOnly">
        /// Predicate that returns <c>true</c> when only the token keyword should be highlighted
        /// (not the full line) for a given <see cref="CommentType"/>.
        /// </param>
        public static List<CommentSegment> ParseBlockCommentSegments(
            SnapshotSpan span,
            string opener,
            string closer,
            Func<CommentType, bool> isKeywordOnly)
        {
            var segments    = new List<CommentSegment>();
            var snapshot    = span.Snapshot;
            var firstLineNo = snapshot.GetLineFromPosition(span.Start).LineNumber;
            var lastLineNo  = snapshot.GetLineFromPosition(span.End).LineNumber;

            var currentType = CommentType.Normal;

            for (var lineNo = firstLineNo; lineNo <= lastLineNo; lineNo++)
            {
                var line     = snapshot.GetLineFromLineNumber(lineNo);
                var lineText = line.GetText();
                bool isFirst = lineNo == firstLineNo;
                bool isLast  = lineNo == lastLineNo;

                // The closing line is usually just "*/" — skip it entirely.
                if (isLast && IsOnlyCloser(lineText, closer))
                    break;

                // ── Determine where usable content starts on this line ─────────────────────
                int rawContentStart = isFirst ? opener.Length : 0;
                int contentStart    = SkipWhitespaceAndJavadocStar(lineText, rawContentStart, closer);

                // ── Determine where usable content ends on this line ──────────────────────
                int contentEnd = isLast
                    ? LastCharBeforeCloser(lineText, contentStart, closer)
                    : lineText.Length;

                int contentLen = contentEnd - contentStart;

                if (contentLen <= 0 || string.IsNullOrWhiteSpace(lineText.Substring(contentStart, contentLen)))
                {
                    // Empty / whitespace → reset current section
                    currentType = CommentType.Normal;
                    continue;
                }

                // ── Token detection ───────────────────────────────────────────────────────
                string rawContent    = lineText.Substring(contentStart, contentLen);
                string trimmedForTok = rawContent.TrimStart();
                var    matchedType   = TokenMatcher.Match(trimmedForTok);

                if (matchedType != CommentType.Normal)
                {
                    // Start of a new classified section.
                    currentType = matchedType;

                    int leadingSpace = rawContent.Length - trimmedForTok.Length;
                    int tokenStart   = contentStart + leadingSpace;

                    if (isKeywordOnly(matchedType))
                    {
                        // Keyword-only mode: highlight just the token word.
                        string token = TokenMatcher.GetMatchedToken(trimmedForTok);
                        if (token != null)
                            segments.Add(new CommentSegment(
                                new SnapshotSpan(snapshot, line.Start + tokenStart, token.Length),
                                matchedType));
                    }
                    else
                    {
                        // Full-line mode: highlight from token to end of usable content.
                        int len = contentEnd - tokenStart;
                        if (len > 0)
                            segments.Add(new CommentSegment(
                                new SnapshotSpan(snapshot, line.Start + tokenStart, len),
                                matchedType));
                    }
                }
                else if (currentType != CommentType.Normal)
                {
                    // Continuation of the current section.
                    if (!isKeywordOnly(currentType))
                    {
                        int firstChar = IndexOfFirstChar(lineText, contentStart);
                        if (firstChar < 0) firstChar = contentStart;
                        int len = contentEnd - firstChar;
                        if (len > 0)
                            segments.Add(new CommentSegment(
                                new SnapshotSpan(snapshot, line.Start + firstChar, len),
                                currentType));
                    }
                    // In keyword-only mode continuation lines are not highlighted.
                }
                // else: no token, no active section → leave uncoloured.
            }

            return segments;
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Span reconstruction helpers (used by JavaScript / TypeScript parser)
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a <see cref="SnapshotSpan"/> covering the complete source line containing
        /// <paramref name="source"/>, starting from the first occurrence of
        /// <paramref name="startString"/> on that line.
        /// </summary>
        public static SnapshotSpan CompleteSingleLineCommentSpan(SnapshotSpan source, string startString)
        {
            if (!source.GetText().Contains(startString))
                throw new ArgumentException(
                    $"The SnapshotSpan does not contain \"{startString}\".", nameof(startString));

            var line   = source.Snapshot.GetLineFromPosition(source.Start);
            var offset = line.GetText().IndexOf(startString, StringComparison.OrdinalIgnoreCase);

            return new SnapshotSpan(source.Snapshot, line.Start + offset, line.Length - offset);
        }

        /// <summary>
        /// Returns the list of <see cref="SnapshotSpan"/> objects covering a single-line
        /// delimited comment (one that opens and closes on the same source line).
        /// </summary>
        public static List<SnapshotSpan> CompleteDelimitedCommentSpan(
            SnapshotSpan source, string start, string end)
        {
            if (!source.GetText().Contains(start))
                throw new ArgumentException(
                    $"The SnapshotSpan does not contain \"{start}\".", nameof(start));

            var spans    = new List<SnapshotSpan>();
            var line     = source.Snapshot.GetLineFromPosition(source.Start);
            var lineText = line.GetText();

            if (lineText.Contains(start) && lineText.Contains(end))
            {
                var startIdx = lineText.IndexOf(start, StringComparison.OrdinalIgnoreCase);
                var endIdx   = lineText.IndexOf(end,   StringComparison.OrdinalIgnoreCase);
                spans.Add(new SnapshotSpan(source.Snapshot,
                                           line.Start + startIdx,
                                           endIdx - startIdx + end.Length));
            }

            return spans;
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Private helpers
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when <paramref name="lineText"/> (trimmed) contains only the
        /// closing delimiter or is empty.
        /// </summary>
        private static bool IsOnlyCloser(string lineText, string closer)
        {
            var t = lineText.Trim();
            return t.Length == 0 || t == closer;
        }

        /// <summary>
        /// Advances past leading whitespace starting at <paramref name="start"/>, then optionally
        /// past a single leading <c>*</c> that is not the start of the closing delimiter
        /// (Javadoc-style <c> * content</c>).  Returns the index of the first real content
        /// character.
        /// </summary>
        private static int SkipWhitespaceAndJavadocStar(string lineText, int start, string closer)
        {
            int idx = start;

            // Skip whitespace.
            while (idx < lineText.Length && char.IsWhiteSpace(lineText[idx]))
                idx++;

            // Skip a single leading '*' that is not the start of the closer.
            if (idx < lineText.Length && lineText[idx] == '*')
            {
                // Don't strip if this is "*/", "*)", etc.
                bool isCloserHere = lineText.IndexOf(closer, idx, StringComparison.OrdinalIgnoreCase) == idx;
                if (!isCloserHere)
                {
                    idx++; // consume '*'
                    // Skip whitespace after '*'.
                    while (idx < lineText.Length && char.IsWhiteSpace(lineText[idx]))
                        idx++;
                }
            }

            return idx;
        }

        /// <summary>
        /// Returns the exclusive end position of usable content on the last line — just before
        /// the closing delimiter and any trailing whitespace.
        /// Returns <paramref name="lineText"/>.Length if the closer is not found.
        /// </summary>
        private static int LastCharBeforeCloser(string lineText, int contentStart, string closer)
        {
            int closerIdx = lineText.IndexOf(closer, StringComparison.OrdinalIgnoreCase);
            if (closerIdx < 0) return lineText.Length;

            // Walk backward from the character before "*/" to find the last non-whitespace.
            int lastNonWs = closerIdx - 1;
            while (lastNonWs >= contentStart && char.IsWhiteSpace(lineText[lastNonWs]))
                lastNonWs--;

            return lastNonWs >= contentStart ? lastNonWs + 1 : contentStart;
        }

        /// <summary>
        /// Returns the index of the first non-whitespace character at or after
        /// <paramref name="startFrom"/>, or -1 if none exists.
        /// </summary>
        private static int IndexOfFirstChar(string s, int startFrom)
        {
            for (int i = startFrom; i < s.Length; i++)
                if (!char.IsWhiteSpace(s[i])) return i;
            return -1;
        }
    }
}
