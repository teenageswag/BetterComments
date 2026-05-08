// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;

namespace BetterComments.Options
{
    /// <summary>
    /// Code-behind for the Better Comments General options page control.
    /// Binds to <see cref="BetterCommentsSettings.Instance"/> so that all XAML bindings resolve
    /// against the live singleton settings object.
    /// </summary>
    public partial class OptionsGeneralPageControl
    {
        /// <summary>
        /// Initialises the control, sets the data context to the settings singleton, and
        /// populates the font family combo box with system fonts.
        /// </summary>
        public OptionsGeneralPageControl()
        {
            DataContext = BetterCommentsSettings.Instance;
            InitializeComponent();
            FontsComboBox.ItemsSource = GetInstalledFonts();
        }

        private static IEnumerable<string> GetInstalledFonts()
        {
            using (var fonts = new InstalledFontCollection())
            {
                return fonts.Families.Select(f => f.Name).ToList();
            }
        }
    }
}
