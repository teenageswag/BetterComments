// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
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
        private readonly BetterCommentsSettings settings = BetterCommentsSettings.Instance;

        /// <summary>
        /// Raised when tags change.  Not currently fired (tags are recomputed on every request).
        /// </summary>
#pragma warning disable CS0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore CS0067

        /// <summary>
        /// Initialises the tagger with the classification type registry and tag aggregator.
        /// </summary>
        public CommentTagger(IClassificationTypeRegistryService registry,
                             ITagAggregator<IClassificationTag> aggregator)
        {
            classRegistry = registry;
            tagAggregator  = aggregator;
        }

        /// <inheritdoc/>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var snapshot = spans[0].Snapshot;
            var results  = new List<TagSpan<ClassificationTag>>();
            var parser   = CreateCommentParser(snapshot.ContentType);

            if (parser == null) return results; // Unsupported content type

            foreach (var tagSpan in tagAggregator.GetTags(spans)
                                                  .Where(m => m.Tag.IsComment() && !m.Tag.IsXmlDoc()))
            {
                foreach (var span in tagSpan.Span.GetSpans(snapshot).Where(s => parser.IsValidComment(s)))
                {
                    try
                    {
                        results.AddRange(CreateTagSpans(parser.Parse(span)));
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

        private IEnumerable<TagSpan<ClassificationTag>> CreateTagSpans(Comment comment)
        {
            foreach (var seg in comment.Segments)
            {
                if (seg.Type != CommentType.Normal)
                    yield return new TagSpan<ClassificationTag>(seg.Span, CreateTag(seg.Type));
            }
        }

        private ClassificationTag CreateTag(CommentType type)
        {
            switch (type)
            {
                case CommentType.Critical:
                    return new ClassificationTag(classRegistry.GetClassificationType(Constants.CriticalComment));
                case CommentType.Warning:
                    return new ClassificationTag(classRegistry.GetClassificationType(Constants.WarningComment));
                case CommentType.Ideas:
                    return new ClassificationTag(classRegistry.GetClassificationType(Constants.IdeasComment));
                case CommentType.Info:
                    return new ClassificationTag(classRegistry.GetClassificationType(Constants.InfoComment));
                default:
                    return new ClassificationTag(classRegistry.GetClassificationType("comment"));
            }
        }

        /// <summary>
        /// Selects the language-specific parser for the given content type, or returns
        /// <c>null</c> if the content type is not supported by this extension.
        /// </summary>
        private ICommentParser CreateCommentParser(IContentType contentType)
        {
            var s = settings;

            if (contentType.IsOfType(Constants.ContentTypeCSharp))      return new CSharpCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeBasic))       return new VBCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypePython))      return new PythonCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeFSharp))      return new FSharpCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeCpp))         return new CppCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeJavaScript)
             || contentType.IsOfType(Constants.ContentTypeTypeScript))  return new JavaScriptCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeRazorCSharp)) return new MarkupCommentParser(s);

            var name = contentType.TypeName.ToLowerInvariant();
            if (name.Contains(Constants.ContentTypeXaml) || name.Contains(Constants.ContentTypeHtml))
                return new MarkupCommentParser(s);

            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            tagAggregator.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
