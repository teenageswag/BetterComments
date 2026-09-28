// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// Produces <see cref="ClassificationTag"/> spans for the four Better Comments classifications.
    /// Receives pre-classified comment spans from an <see cref="ITagAggregator{T}"/> and delegates
    /// type detection and span computation to the appropriate language-specific
    /// <see cref="ICommentParser"/>.
    /// </summary>
    internal sealed class CommentTagger : ITagger<ClassificationTag>, IDisposable
    {
        private readonly IClassificationTypeRegistryService classRegistry;
        private readonly ITagAggregator<IClassificationTag> tagAggregator;
        private readonly Func<IContentType, ICommentParser> parserFactory;
        private readonly ITextBuffer buffer;
        private readonly ITextView textView;
        private bool disposed;
        private readonly Dictionary<CommentType, ClassificationTag> tagCache = new Dictionary<CommentType, ClassificationTag>();
        private readonly Dictionary<string, ClassificationTag> customTagCache = new Dictionary<string, ClassificationTag>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Raised when source classifications or Better Comments settings change.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Initialises the tagger with the classification type registry, tag aggregator, and parser factory.
        /// </summary>
        public CommentTagger(IClassificationTypeRegistryService registry,
                             ITagAggregator<IClassificationTag> aggregator,
                             Func<IContentType, ICommentParser> parserFactory,
                             ITextBuffer buffer,
                             ITextView textView)
        {
            classRegistry = registry;
            tagAggregator = aggregator;
            this.parserFactory = parserFactory;
            this.buffer = buffer;
            this.textView = textView;
            tagAggregator.TagsChanged += OnSourceTagsChanged;
            SettingsStore.SettingsChanged += OnSettingsChanged;
            textView.Closed += OnViewClosed;
        }

        private void OnViewClosed(object sender, EventArgs e) => Dispose();

        private void OnSourceTagsChanged(object sender, TagsChangedEventArgs e)
        {
            foreach (var span in e.Span.GetSpans(buffer.CurrentSnapshot))
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        private void OnSettingsChanged()
        {
            tagCache.Clear();
            customTagCache.Clear();
            var snapshot = buffer.CurrentSnapshot;
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
        }

        /// <inheritdoc/>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                return Enumerable.Empty<ITagSpan<ClassificationTag>>();

            var snapshot = spans[0].Snapshot;
            var results  = new List<TagSpan<ClassificationTag>>();
            var parser   = parserFactory(snapshot.ContentType);

            if (parser == null) return results; // Unsupported content type

            var yieldedSpans = new HashSet<SnapshotSpan>();

            foreach (var tagSpan in tagAggregator.GetTags(spans)
                                                  .Where(m => m.Tag.IsComment() && !m.Tag.IsXmlDoc()))
            {
                foreach (var span in tagSpan.Span.GetSpans(snapshot).Where(s => parser.IsValidComment(s)))
                {
                    try
                    {
                        var comment = parser.Parse(span);
                        foreach (var seg in comment.Segments)
                        {
                            if (seg.Type != CommentType.Normal
                                && spans.Any(requested => requested.IntersectsWith(seg.Span))
                                && yieldedSpans.Add(seg.Span))
                            {
                                var tag = CreateTag(seg.Type, seg.CustomTagId);
                                if (tag != null)
                                    results.Add(new TagSpan<ClassificationTag>(seg.Span, tag));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[BetterComments] Exception while tagging: {ex}");
                    }
                }
            }

            return results;
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Private helpers
        // ──────────────────────────────────────────────────────────────────────────────────────

        private ClassificationTag CreateTag(CommentType type, string customTagId)
        {
            // Handle custom tags
            if (customTagId != null)
                return CreateCustomTag(customTagId);

            // Handle built-in types
            if (tagCache.TryGetValue(type, out var cached))
                return cached;

            ClassificationTag tag;
            switch (type)
            {
                case CommentType.Critical:
                    tag = CreateClassificationTag(Constants.CriticalComment);
                    break;
                case CommentType.Warning:
                    tag = CreateClassificationTag(Constants.WarningComment);
                    break;
                case CommentType.Ideas:
                    tag = CreateClassificationTag(Constants.IdeasComment);
                    break;
                case CommentType.Info:
                    tag = CreateClassificationTag(Constants.InfoComment);
                    break;
                default:
                    tag = CreateClassificationTag("comment");
                    break;
            }

            tagCache[type] = tag;
            return tag;
        }

        private ClassificationTag CreateCustomTag(string customTagId)
        {
            if (customTagCache.TryGetValue(customTagId, out var cached))
                return cached;

            // Look up the custom tag definition to get its classification name
            var settings = BetterCommentsSettings.Instance;
            var customTag = settings.CustomTags.GetByTag(customTagId);

            string classificationName;
            if (customTag != null && customTag.UseCustomColor)
            {
                // Use the custom tag's own classification
                classificationName = customTag.GetClassificationName();
            }
            else if (customTag != null)
            {
                // Use the mapped type's classification
                classificationName = customTag.GetClassificationName();
            }
            else
            {
                // Fallback to Info if custom tag not found
                classificationName = Constants.InfoComment;
            }

            var tag = CreateClassificationTag(classificationName);
            customTagCache[customTagId] = tag;
            return tag;
        }

        private ClassificationTag CreateClassificationTag(string classificationName)
        {
            var classificationType = classRegistry.GetClassificationType(classificationName)
                                  ?? classRegistry.GetClassificationType("comment");
            return classificationType == null ? null : new ClassificationTag(classificationType);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            textView.Closed -= OnViewClosed;
            tagAggregator.TagsChanged -= OnSourceTagsChanged;
            SettingsStore.SettingsChanged -= OnSettingsChanged;
            tagAggregator.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
