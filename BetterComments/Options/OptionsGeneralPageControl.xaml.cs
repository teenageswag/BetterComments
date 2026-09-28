// Copyright (c) Omar Rwemi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using BetterComments.CommentsTagging;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterComments.Options
{
    /// <summary>
    /// Code-behind for the Better Comments options page.
    /// </summary>
    public partial class OptionsGeneralPageControl
    {
        private static readonly HashSet<string> BuiltInTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ERROR", "ERR", "FIX", "FIXME", "WARNING", "WARN", "TODO", "IDEA", "OPTIMIZE", "NOTE", "INFO"
        };

        public OptionsGeneralPageControl()
        {
            InitializeComponent();
            DataContext = BetterCommentsSettings.Instance;
            FontsComboBox.ItemsSource = GetInstalledFonts();
            CustomTagsListBox.ItemsSource = BetterCommentsSettings.Instance.CustomTags.CustomTags;
        }

        private static IEnumerable<string> GetInstalledFonts()
        {
            using (var fonts = new InstalledFontCollection())
                return fonts.Families.Select(family => family.Name).OrderBy(name => name).ToList();
        }

        private void CustomTagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveCustomTagButton.IsEnabled = CustomTagsListBox.SelectedItem is CustomTagDefinition;
        }

        private void AddCustomTag_Click(object sender, RoutedEventArgs e)
        {
            var tagName = NewTagTextBox.Text?.Trim().TrimEnd(':').ToUpperInvariant();
            if (!IsValidTag(tagName))
            {
                ShowValidation("Метка должна начинаться с буквы и содержать только буквы, цифры или _.");
                return;
            }

            if (BuiltInTags.Contains(tagName))
            {
                ShowValidation("Это слово уже зарезервировано встроенной меткой.");
                return;
            }

            var settings = BetterCommentsSettings.Instance;
            if (settings.CustomTags.GetByTag(tagName) != null)
            {
                ShowValidation($"Метка {tagName}: уже существует.");
                return;
            }

            settings.CustomTags.Add(new CustomTagDefinition
            {
                Tag = tagName,
                MappedType = CommentType.Info,
                Enabled = true
            });

            NewTagTextBox.Clear();
            TagValidationText.Visibility = Visibility.Collapsed;
            NewTagTextBox.Focus();
        }

        private static bool IsValidTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || !char.IsLetter(tag[0]))
                return false;

            return tag.All(character => char.IsLetterOrDigit(character) || character == '_');
        }

        private void ShowValidation(string message)
        {
            TagValidationText.Text = message;
            TagValidationText.Visibility = Visibility.Visible;
        }

        private void RemoveCustomTag_Click(object sender, RoutedEventArgs e)
        {
            if (CustomTagsListBox.SelectedItem is CustomTagDefinition selectedTag)
                BetterCommentsSettings.Instance.CustomTags.Remove(selectedTag.Tag);
        }

        private void NewTagTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddCustomTag_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
