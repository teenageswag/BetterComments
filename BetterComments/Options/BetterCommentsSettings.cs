using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BetterComments.Options
{
    public class BetterCommentsSettings : PropertyChangeNotifier, ISettings
    {
        public static BetterCommentsSettings Instance { get; } = new BetterCommentsSettings();

        // Used by SettingsStore to namespace the settings
        public string Key => "BetterComments";

        public BetterCommentsSettings()
        {
            ResetToDefaults();
            customTags = new CustomTagSettings();
            customTags.TagsChanged += OnCustomTagsChanged;

            // Load saved settings from VS store if they exist
            SettingsStore.LoadSettings(this);
        }

        internal void ResetToDefaults()
        {
            Font = string.Empty;
            Size = 0;
            Opacity = 1;
            Italic = false;

            critical = new CommentTypeSettings(defaultBold: true);
            warning = new CommentTypeSettings();
            ideas = new CommentTypeSettings();
            info = new CommentTypeSettings();

            OnPropertyChanged(nameof(CriticalBold));
            OnPropertyChanged(nameof(CriticalUnderline));
            OnPropertyChanged(nameof(CriticalHighlightKeywordOnly));
            OnPropertyChanged(nameof(WarningBold));
            OnPropertyChanged(nameof(WarningUnderline));
            OnPropertyChanged(nameof(WarningHighlightKeywordOnly));
            OnPropertyChanged(nameof(IdeasBold));
            OnPropertyChanged(nameof(IdeasUnderline));
            OnPropertyChanged(nameof(IdeasHighlightKeywordOnly));
            OnPropertyChanged(nameof(InfoBold));
            OnPropertyChanged(nameof(InfoUnderline));
            OnPropertyChanged(nameof(InfoHighlightKeywordOnly));

            if (customTags != null)
                customTags.LoadFrom(null);
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
            set => SetField(ref size, double.IsNaN(value) ? 0 : Math.Max(-3, Math.Min(3, value)));
        }

        private double opacity;
        [Setting]
        public double Opacity
        {
            get => opacity;
            set => SetField(ref opacity, double.IsNaN(value) ? 1 : Math.Max(0.1, Math.Min(1, value)));
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
        internal string CustomTagsList
        {
            get => string.Join(";", customTags.CustomTags.Select(tag => string.Join(",",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(tag.Tag ?? string.Empty)),
                ((int)tag.MappedType).ToString(CultureInfo.InvariantCulture),
                tag.UseCustomColor ? "1" : "0",
                tag.Enabled ? "1" : "0")));
            set
            {
                var definitions = new List<CustomTagDefinition>();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    foreach (var item in value.Split(';'))
                    {
                        var parts = item.Split(',');
                        int type;
                        if (parts.Length != 4
                            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out type)
                            || !Enum.IsDefined(typeof(CommentsTagging.CommentType), type))
                            continue;

                        try
                        {
                            definitions.Add(new CustomTagDefinition
                            {
                                Tag = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0])),
                                MappedType = (CommentsTagging.CommentType)type,
                                UseCustomColor = parts[2] == "1",
                                Enabled = parts[3] == "1"
                            });
                        }
                        catch (FormatException)
                        {
                            // Ignore a malformed saved entry and continue loading valid tags.
                        }
                    }
                }

                customTags.LoadFrom(definitions);
            }
        }
    }
}
