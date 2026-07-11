// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace BetterComments.CommentsTagging
{
    /// <summary>
    /// MEF export that creates a <see cref="CommentTagger"/> for every text view whose content
    /// type derives from <c>code</c>.  Using <see cref="IViewTaggerProvider"/> (rather than the
    /// buffer-level <c>ITaggerProvider</c>) ensures each view gets its own tagger instance and
    /// avoids cross-view state sharing.
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(Constants.ContentTypeCode)]
    [TagType(typeof(ClassificationTag))]
    internal sealed class CommentTaggerProvider : IViewTaggerProvider
    {
#pragma warning disable CS0649 // Fields are assigned by MEF

        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        [Import]
        internal IBufferTagAggregatorFactoryService BufferTagAggregatorFactory;

#pragma warning restore CS0649

        private readonly ConcurrentDictionary<string, ICommentParser> parserCache =
            new ConcurrentDictionary<string, ICommentParser>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var tagAggregator = BufferTagAggregatorFactory.CreateTagAggregator<IClassificationTag>(buffer);

            Debug.WriteLine("[BetterComments] CommentTagger created.");

            return new CommentTagger(ClassificationRegistry, tagAggregator, GetOrCreateParser) as ITagger<T>;
        }

        private ICommentParser GetOrCreateParser(IContentType contentType)
        {
            var typeName = contentType.TypeName;
            return parserCache.GetOrAdd(typeName, _ => CreateCommentParser(contentType));
        }

        private static ICommentParser CreateCommentParser(IContentType contentType)
        {
            var s = BetterCommentsSettings.Instance;

            if (contentType.IsOfType(Constants.ContentTypeCSharp))      return new CSharpCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeBasic))       return new VBCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypePython))      return new PythonCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeFSharp))      return new FSharpCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeCpp))         return new CppCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeJavaScript)
             || contentType.IsOfType(Constants.ContentTypeTypeScript))  return new JavaScriptCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeRazorCSharp)) return new MarkupCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeRust))        return new RustCommentParser(s);
            if (contentType.IsOfType(Constants.ContentTypeGo))          return new GoCommentParser(s);

            var name = contentType.TypeName.ToLowerInvariant();
            if (name.Contains(Constants.ContentTypeXaml) || name.Contains(Constants.ContentTypeHtml))
                return new MarkupCommentParser(s);

            return null;
        }
    }
}
