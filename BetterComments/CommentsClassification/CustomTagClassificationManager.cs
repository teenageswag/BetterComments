using BetterComments.Options;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace BetterComments.CommentsClassification
{
    /// <summary>
    /// Manages dynamically created classification types for custom tags.
    /// </summary>
    internal sealed class CustomTagClassificationManager
    {
        private readonly IClassificationTypeRegistryService registry;
        private readonly IClassificationFormatMap formatMap;
        private readonly Dictionary<string, IClassificationType> registeredTypes =
            new Dictionary<string, IClassificationType>(StringComparer.OrdinalIgnoreCase);

        // Default colors for custom tags (based on hash of tag name for consistency)
        private static readonly Color[] DefaultColors = new[]
        {
            (Color)ColorConverter.ConvertFromString("#FF6B6B"), // Red
            (Color)ColorConverter.ConvertFromString("#4ECDC4"), // Teal
            (Color)ColorConverter.ConvertFromString("#45B7D1"), // Blue
            (Color)ColorConverter.ConvertFromString("#96CEB4"), // Green
            (Color)ColorConverter.ConvertFromString("#FFEAA7"), // Yellow
            (Color)ColorConverter.ConvertFromString("#DDA0DD"), // Plum
            (Color)ColorConverter.ConvertFromString("#98D8C8"), // Mint
            (Color)ColorConverter.ConvertFromString("#F7DC6F"), // Gold
        };

        public CustomTagClassificationManager(
            IClassificationTypeRegistryService registry,
            IClassificationFormatMap formatMap)
        {
            this.registry = registry;
            this.formatMap = formatMap;
        }

        /// <summary>
        /// Gets or creates a classification type for a custom tag.
        /// </summary>
        public IClassificationType GetOrCreateClassificationType(CustomTagDefinition customTag)
        {
            if (!customTag.UseCustomColor)
                return null;

            var classificationName = customTag.GetClassificationName();

            if (registeredTypes.TryGetValue(classificationName, out var existing))
                return existing;

            // Try to get existing classification type first
            var classificationType = registry.GetClassificationType(classificationName);

            if (classificationType == null)
            {
                // Create a new classification type dynamically
                // Note: This requires the classification type to be registered via MEF
                // or through the registry. In practice, for custom tags with custom colors,
                // users will need to configure colors in Tools > Options > Fonts and Colors.
                classificationType = registry.GetClassificationType(classificationName);
            }

            if (classificationType != null)
            {
                registeredTypes[classificationName] = classificationType;
                EnsureDefaultFormatExists(classificationName, classificationType, customTag.Tag);
            }

            return classificationType;
        }

        /// <summary>
        /// Ensures a default format definition exists for the classification type.
        /// </summary>
        private void EnsureDefaultFormatExists(string classificationName, IClassificationType classificationType, string tagName)
        {
            var existingFormat = formatMap.GetExplicitTextProperties(classificationType);
            if (existingFormat != null && !existingFormat.TypefaceEmpty)
                return; // Format already exists

            // Get a default color based on the tag name
            var color = GetDefaultColor(tagName);

            // Create a simple format definition
            var textProperties = formatMap.GetTextProperties(classificationType);
            textProperties = textProperties.SetForegroundOpacity(1.0);
            formatMap.SetTextProperties(classificationType, textProperties);
        }

        /// <summary>
        /// Gets a default color for a custom tag based on its name.
        /// </summary>
        private static Color GetDefaultColor(string tagName)
        {
            int hash = StringComparer.OrdinalIgnoreCase.GetHashCode(tagName);
            int index = Math.Abs(hash) % DefaultColors.Length;
            return DefaultColors[index];
        }

        /// <summary>
        /// Removes registered classification types for tags that no longer exist.
        /// </summary>
        public void Cleanup(IEnumerable<CustomTagDefinition> currentTags)
        {
            var currentClassNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tag in currentTags)
            {
                if (tag.UseCustomColor)
                    currentClassNames.Add(tag.GetClassificationName());
            }

            var toRemove = new List<string>();
            foreach (var kvp in registeredTypes)
            {
                if (!currentClassNames.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }

            foreach (var key in toRemove)
                registeredTypes.Remove(key);
        }
    }
}
