// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.CommentsTagging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Controls;

namespace BetterComments.Options
{
    /// <summary>
    /// Code-behind for the Better Comments General options page control.
    /// Binds to <see cref="BetterCommentsSettings.Instance"/> so that all XAML bindings resolve
    /// against the live singleton settings object.
    /// </summary>
    public partial class OptionsGeneralPageControl
    {
        private readonly ObservableCollection<CustomTagDefinition> customTagsCollection;

        /// <summary>
        /// Initialises the control, sets the data context to the settings singleton, and
        /// populates the font family combo box with system fonts.
        /// </summary>
        public OptionsGeneralPageControl()
        {
            DataContext = BetterCommentsSettings.Instance;
            InitializeComponent();
            FontsComboBox.ItemsSource = GetInstalledFonts();

            // Initialize custom tags collection
            customTagsCollection = new ObservableCollection<CustomTagDefinition>(
                BetterCommentsSettings.Instance.CustomTags.CustomTags);
            CustomTagsListBox.ItemsSource = customTagsCollection;
        }

        private static IEnumerable<string> GetInstalledFonts()
        {
            using (var fonts = new InstalledFontCollection())
            {
                return fonts.Families.Select(f => f.Name).ToList();
            }
        }

        private void CustomTagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection changed handler (can be used for future features)
        }

        private void AddCustomTag_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var tagName = NewTagTextBox.Text?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(tagName))
                return;

            // Check if tag already exists
            if (customTagsCollection.Any(t =>
                string.Equals(t.Tag, tagName, StringComparison.OrdinalIgnoreCase)))
            {
                System.Windows.MessageBox.Show(
                    $"Tag '{tagName}' already exists.",
                    "Better Comments",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            var newTag = new CustomTagDefinition
            {
                Tag = tagName,
                MappedType = CommentType.Info,
                UseCustomColor = false,
                Enabled = true
            };

            customTagsCollection.Add(newTag);
            BetterCommentsSettings.Instance.CustomTags.Add(newTag);
            NewTagTextBox.Text = string.Empty;
        }

        private void RemoveCustomTag_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CustomTagsListBox.SelectedItem is CustomTagDefinition selectedTag)
            {
                customTagsCollection.Remove(selectedTag);
                BetterCommentsSettings.Instance.CustomTags.Remove(selectedTag.Tag);
            }
        }
    }
}
