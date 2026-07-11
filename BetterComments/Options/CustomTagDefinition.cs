using BetterComments.CommentsTagging;
using System;
using System.ComponentModel;

namespace BetterComments.Options
{
    /// <summary>
    /// Defines a custom comment tag that users can add to BetterComments.
    /// </summary>
    internal class CustomTagDefinition : PropertyChangeNotifier, IEquatable<CustomTagDefinition>
    {
        private string tag;
        /// <summary>
        /// The tag keyword (e.g., "REVIEW", "HACK", "DEPRECATED").
        /// The colon is added automatically during matching.
        /// </summary>
        public string Tag
        {
            get => tag;
            set => SetField(ref tag, value?.ToUpperInvariant());
        }

        private CommentType mappedType = CommentType.Info;
        /// <summary>
        /// The built-in comment type this custom tag maps to.
        /// Used when UseCustomColor is false.
        /// </summary>
        public CommentType MappedType
        {
            get => mappedType;
            set => SetField(ref mappedType, value);
        }

        private bool useCustomColor;
        /// <summary>
        /// When true, this tag uses its own classification color (added to Fonts & Colors).
        /// When false, it inherits the color from MappedType.
        /// </summary>
        public bool UseCustomColor
        {
            get => useCustomColor;
            set => SetField(ref useCustomColor, value);
        }

        private bool enabled = true;
        /// <summary>
        /// Whether this custom tag is currently active.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set => SetField(ref enabled, value);
        }

        /// <summary>
        /// Gets the classification name for this custom tag.
        /// Returns a built-in classification name if UseCustomColor is false,
        /// or a custom classification name if UseCustomColor is true.
        /// </summary>
        public string GetClassificationName()
        {
            if (!UseCustomColor)
            {
                switch (MappedType)
                {
                    case CommentType.Critical: return Constants.CriticalComment;
                    case CommentType.Warning:  return Constants.WarningComment;
                    case CommentType.Ideas:    return Constants.IdeasComment;
                    case CommentType.Info:     return Constants.InfoComment;
                    default:                   return Constants.InfoComment;
                }
            }

            return $"{Constants.CustomTagPrefix}{Tag}";
        }

        public bool Equals(CustomTagDefinition other)
        {
            if (other is null) return false;
            return string.Equals(Tag, other.Tag, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => Equals(obj as CustomTagDefinition);
        public override int GetHashCode() => Tag?.ToUpperInvariant().GetHashCode() ?? 0;
    }
}
