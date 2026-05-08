// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Windows;

namespace BetterComments.Options
{
    /// <summary>
    /// Base class for Better Comments options pages.  Handles saving/reloading settings when
    /// the user applies or cancels the Options dialog.
    /// </summary>
    public abstract class OptionsPageBase : UIElementDialogPage
    {
        private bool applied;

        /// <inheritdoc/>
        protected override abstract UIElement Child { get; }

        /// <inheritdoc/>
        protected override void OnActivate(CancelEventArgs e)
        {
            applied = false;
            base.OnActivate(e);
        }

        /// <inheritdoc/>
        protected override void OnApply(PageApplyEventArgs e)
        {
            applied = true;
            e.ApplyBehavior = ApplyKind.Apply;
            base.OnApply(e);
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            if (applied)
                SettingsStore.SaveSettings(BetterCommentsSettings.Instance);
            else
                SettingsStore.LoadSettings(BetterCommentsSettings.Instance); // Discard unsaved changes

            applied = false;
            base.OnClosed(e);
        }
    }
}
