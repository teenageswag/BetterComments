// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace BetterComments.CommentsViewCustomization
{
    /// <summary>
    /// Applies runtime formatting overrides (font family, size offset, italic, opacity, bold,
    /// underline) to all comment classifications in a single WPF text view.
    ///
    /// Global settings (font, size, italic, opacity) are applied to every classification whose
    /// name contains "comment".  Per-classification settings (bold, underline) are applied only
    /// to the four Better Comments types.
    /// </summary>
    internal sealed class CommentViewDecorator
    {
        private bool isDecorating;

        private readonly IClassificationFormatMap formatMap;
        private readonly IClassificationTypeRegistryService regService;
        private readonly BetterCommentsSettings settings = BetterCommentsSettings.Instance;

        /// <summary>
        /// Well-known VS classification names that are treated as "comment" types.
        /// Any classification whose name <em>contains</em> "comment" and is not in this list is
        /// picked up by the "unknown" pass.
        /// </summary>
        private static readonly HashSet<string> KnownCommentTypes = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "comment",
            "xml doc comment",
            "vb xml doc comment",
            "xml comment",
            "html comment",
            "xaml comment",
        };

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Factory
        // ──────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the existing decorator for <paramref name="view"/>, or creates and attaches a
        /// new one.
        /// </summary>
        public static CommentViewDecorator Create(
            ITextView view,
            IClassificationFormatMap map,
            IClassificationTypeRegistryService service)
        {
            return view.Properties.GetOrCreateSingletonProperty(
                () => new CommentViewDecorator(view, map, service));
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Constructor
        // ──────────────────────────────────────────────────────────────────────────────────────

        private CommentViewDecorator(
            ITextView view,
            IClassificationFormatMap map,
            IClassificationTypeRegistryService service)
        {
            view.GotAggregateFocus += OnViewGotFocus;
            SettingsStore.SettingsSaved += OnSettingsSaved;

            formatMap  = map;
            regService = service;

            Decorate();
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Event handlers
        // ──────────────────────────────────────────────────────────────────────────────────────

        private void OnSettingsSaved()
        {
            if (!isDecorating) Decorate();
        }

        private void OnViewGotFocus(object sender, EventArgs e)
        {
            if (sender is ITextView view)
                view.GotAggregateFocus -= OnViewGotFocus;

            if (!isDecorating) Decorate();
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Decoration logic
        // ──────────────────────────────────────────────────────────────────────────────────────

        private void Decorate()
        {
            try
            {
                isDecorating = true;
                formatMap.BeginBatchUpdate();

                DecorateKnownCommentTypes();
                DecorateUnknownCommentTypes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BetterComments] Exception while decorating: {ex.Message}");
            }
            finally
            {
                formatMap.EndBatchUpdate();
                isDecorating = false;
            }
        }

        private void DecorateKnownCommentTypes()
        {
            foreach (var name in KnownCommentTypes)
            {
                var classificationType = regService.GetClassificationType(name);
                if (classificationType != null)
                    SetProperties(classificationType);
            }
        }

        private void DecorateUnknownCommentTypes()
        {
            foreach (var ct in formatMap.CurrentPriorityOrder)
            {
                if (ct == null) continue;

                var name = ct.Classification.ToLowerInvariant();
                if (name.Contains("comment") && !KnownCommentTypes.Contains(ct.Classification))
                    SetProperties(ct);
            }
        }

        /// <summary>
        /// Applies the current settings to one classification type.
        /// Global settings (font/size/italic/opacity) are always applied.
        /// Bold and underline are applied only for the four Better Comments classifications.
        /// </summary>
        private void SetProperties(IClassificationType classificationType)
        {
            var props    = formatMap.GetTextProperties(classificationType);
            var name     = classificationType.Classification;
            var isBCType = IsBetterCommentsType(name);

            // ── Bold & underline (Better Comments types only) ─────────────────────────────────
            bool isBold      = isBCType && GetBold(name);
            bool isUnderline = isBCType && GetUnderline(name);

            // ── Font family ───────────────────────────────────────────────────────────────────
            var currentTf  = props.TypefaceEmpty ? null : props.Typeface;
            var fontFamily = !string.IsNullOrWhiteSpace(settings.Font)
                           ? new FontFamily(settings.Font)
                           : (currentTf?.FontFamily ?? new FontFamily());

            var typeface = new Typeface(
                fontFamily,
                settings.Italic ? FontStyles.Italic : FontStyles.Normal,
                isBold         ? FontWeights.Bold   : FontWeights.Normal,
                FontStretches.Normal);

            props = props.SetTypeface(typeface);

            // ── Font size ─────────────────────────────────────────────────────────────────────
            var targetSize = GetEditorTextSize() + settings.Size;
            if (Math.Abs(targetSize - props.FontRenderingEmSize) > 0.01)
                props = props.SetFontRenderingEmSize(targetSize);

            // ── Opacity ───────────────────────────────────────────────────────────────────────
            if (settings.Opacity >= 0.1 && settings.Opacity <= 1.0)
                props = props.SetForegroundOpacity(settings.Opacity);

            // ── Text decorations (underline) ──────────────────────────────────────────────────
            if (isBCType)
            {
                var decorations = new TextDecorationCollection();
                if (isUnderline)
                    decorations.Add(TextDecorations.Underline[0]);
                props = props.SetTextDecorations(decorations);
            }

            formatMap.SetTextProperties(classificationType, props);
        }

        // ──────────────────────────────────────────────────────────────────────────────────────
        //  Helpers
        // ──────────────────────────────────────────────────────────────────────────────────────

        private double GetEditorTextSize()
        {
            var textType = regService.GetClassificationType("text");
            return textType != null
                ? formatMap.GetTextProperties(textType).FontRenderingEmSize
                : 12.0;
        }

        private static bool IsBetterCommentsType(string name)
            => name == Constants.CriticalComment
            || name == Constants.WarningComment
            || name == Constants.IdeasComment
            || name == Constants.InfoComment;

        private bool GetBold(string name)
        {
            if (name == Constants.CriticalComment) return settings.CriticalBold;
            if (name == Constants.WarningComment)  return settings.WarningBold;
            if (name == Constants.IdeasComment)    return settings.IdeasBold;
            if (name == Constants.InfoComment)     return settings.InfoBold;
            return false;
        }

        private bool GetUnderline(string name)
        {
            if (name == Constants.CriticalComment) return settings.CriticalUnderline;
            if (name == Constants.WarningComment)  return settings.WarningUnderline;
            if (name == Constants.IdeasComment)    return settings.IdeasUnderline;
            if (name == Constants.InfoComment)     return settings.InfoUnderline;
            return false;
        }
    }
}
