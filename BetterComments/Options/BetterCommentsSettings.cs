using System.Collections.Generic;
using System.ComponentModel;

namespace BetterComments.Options
{
    public class BetterCommentsSettings : PropertyChangeNotifier, ISettings
    {
        public static BetterCommentsSettings Instance { get; } = new BetterCommentsSettings();

        // Used by SettingsStore to namespace the settings
        public string Key => "BetterComments";

        public BetterCommentsSettings()
        {
            // Initialise default settings
            font = "";
            size = 0;
            opacity = 1.0;
            italic = false;

            critical = new CommentTypeSettings(defaultBold: true);
            warning = new CommentTypeSettings();
            ideas = new CommentTypeSettings();
            info = new CommentTypeSettings();

            customTags = new CustomTagSettings();
            customTags.TagsChanged += OnCustomTagsChanged;

            // Load saved settings from VS store if they exist
            SettingsStore.LoadSettings(this);
        }

        private void OnCustomTagsChanged()
        {
            // Update TokenMatcher with new custom tags
            CommentsTagging.TokenMatcher.UpdateCustomTags(customTags.CustomTags);
        }

        // Global Settings
        private string font;
        [Setting]
        public string Font
        {
            get => font;
            set => SetField(ref font, value);
        }

        private double size;
        [Setting]
        public double Size
        {
            get => size;
            set => SetField(ref size, value);
        }

        private double opacity;
        [Setting]
        public double Opacity
        {
            get => opacity;
            set => SetField(ref opacity, value);
        }

        private bool italic;
        [Setting]
        public bool Italic
        {
            get => italic;
            set => SetField(ref italic, value);
        }

        // Per-type settings
        private CommentTypeSettings critical;
        public CommentTypeSettings Critical => critical;

        private CommentTypeSettings warning;
        public CommentTypeSettings Warning => warning;

        private CommentTypeSettings ideas;
        public CommentTypeSettings Ideas => ideas;

        private CommentTypeSettings info;
        public CommentTypeSettings Info => info;

        // Wrapper properties for backwards compatibility with existing UI bindings
        [Setting]
        public bool CriticalBold
        {
            get => critical.Bold;
            set => critical.Bold = value;
        }

        [Setting]
        public bool CriticalUnderline
        {
            get => critical.Underline;
            set => critical.Underline = value;
        }

        [Setting]
        public bool CriticalHighlightKeywordOnly
        {
            get => critical.HighlightKeywordOnly;
            set => critical.HighlightKeywordOnly = value;
        }

        [Setting]
        public bool WarningBold
        {
            get => warning.Bold;
            set => warning.Bold = value;
        }

        [Setting]
        public bool WarningUnderline
        {
            get => warning.Underline;
            set => warning.Underline = value;
        }

        [Setting]
        public bool WarningHighlightKeywordOnly
        {
            get => warning.HighlightKeywordOnly;
            set => warning.HighlightKeywordOnly = value;
        }

        [Setting]
        public bool IdeasBold
        {
            get => ideas.Bold;
            set => ideas.Bold = value;
        }

        [Setting]
        public bool IdeasUnderline
        {
            get => ideas.Underline;
            set => ideas.Underline = value;
        }

        [Setting]
        public bool IdeasHighlightKeywordOnly
        {
            get => ideas.HighlightKeywordOnly;
            set => ideas.HighlightKeywordOnly = value;
        }

        [Setting]
        public bool InfoBold
        {
            get => info.Bold;
            set => info.Bold = value;
        }

        [Setting]
        public bool InfoUnderline
        {
            get => info.Underline;
            set => info.Underline = value;
        }

        [Setting]
        public bool InfoHighlightKeywordOnly
        {
            get => info.HighlightKeywordOnly;
            set => info.HighlightKeywordOnly = value;
        }

        // Custom tags settings
        private CustomTagSettings customTags;
        internal CustomTagSettings CustomTags => customTags;

        /// <summary>
        /// Gets or sets the custom tags for serialization.
        /// </summary>
        [Setting]
        internal List<CustomTagDefinition> CustomTagsList
        {
            get => customTags.ToList();
            set
            {
                customTags.LoadFrom(value);
                CommentsTagging.TokenMatcher.UpdateCustomTags(customTags.CustomTags);
            }
        }
    }
}
