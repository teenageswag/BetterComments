// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
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

        /// <inheritdoc/>
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var tagAggregator = BufferTagAggregatorFactory.CreateTagAggregator<IClassificationTag>(buffer);

            Debug.WriteLine("[BetterComments] CommentTagger created.");

            // Use GetOrCreateSingletonProperty so that only one tagger is created per view,
            // preventing duplicate classification tags when a buffer is shared across views.
            return textView.Properties
                           .GetOrCreateSingletonProperty(() =>
                               new CommentTagger(ClassificationRegistry, tagAggregator)) as ITagger<T>;
        }
    }
}
