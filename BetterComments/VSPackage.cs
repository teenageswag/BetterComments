// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace BetterComments
{
    /// <summary>
    /// Better Comments VS package.  Registers the Tools → Options page and requests automatic
    /// load when a solution is opened so that the MEF tagger is warmed up without a cold-start
    /// delay on the first file open.
    /// </summary>
    [ProvideOptionPage(typeof(OptionsGeneralPage), "Better Comments", "General", 0, 0, true)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Id, IconResourceID = 400)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuidString)]
    public sealed class VsPackage : AsyncPackage
    {
        /// <summary>Package GUID — must match the entry in source.extension.vsixmanifest.</summary>
        public const string PackageGuidString = "09e59564-c21a-44f8-ae2b-c2bc17facd07";

        /// <inheritdoc/>
        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // Ensure settings are loaded on the background thread so the singleton is ready
            // before the first text view is created.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _ = BetterCommentsSettings.Instance;
        }
    }
}
