// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace BetterComments.CommentsViewCustomization
{
    /// <summary>
    /// MEF export that listens for new WPF text views and attaches a
    /// <see cref="CommentViewDecorator"/> to each one so that font/opacity/bold/underline settings
    /// are applied at view-creation time and whenever settings change.
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(Constants.ContentTypeCode)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class CommentViewCreationListener : IWpfTextViewCreationListener
    {
#pragma warning disable CS0649 // Fields are assigned by MEF

        [Import]
        private IClassificationFormatMapService FormatMapService;

        [Import]
        private IClassificationTypeRegistryService TypeRegistryService;

#pragma warning restore CS0649

        /// <inheritdoc/>
        public void TextViewCreated(IWpfTextView textView)
        {
            // GetOrCreateSingletonProperty guarantees exactly one decorator per view instance.
            textView.Properties.GetOrCreateSingletonProperty(() =>
                CommentViewDecorator.Create(
                    textView,
                    FormatMapService.GetClassificationFormatMap(textView),
                    TypeRegistryService));
        }
    }
}
