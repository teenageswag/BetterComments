using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BetterComments.Options
{
    /// <summary>
    /// Manages the collection of custom tag definitions.
    /// </summary>
    internal class CustomTagSettings
    {
        private readonly ObservableCollection<CustomTagDefinition> customTags = new ObservableCollection<CustomTagDefinition>();

        /// <summary>
        /// Gets the read-only collection of custom tags.
        /// </summary>
        public IReadOnlyList<CustomTagDefinition> CustomTags => customTags;

        /// <summary>
        /// Event raised when the custom tags collection changes.
        /// </summary>
        public event Action TagsChanged;

        public CustomTagSettings()
        {
            customTags.CollectionChanged += (s, e) => TagsChanged?.Invoke();
        }

        /// <summary>
        /// Adds a new custom tag.
        /// </summary>
        public void Add(CustomTagDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (customTags.Any(t => t.Equals(definition)))
                throw new InvalidOperationException($"Tag '{definition.Tag}' already exists.");

            customTags.Add(definition);
        }

        /// <summary>
        /// Removes a custom tag by its tag keyword.
        /// </summary>
        public bool Remove(string tag)
        {
            var existing = customTags.FirstOrDefault(t =>
                string.Equals(t.Tag, tag, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                customTags.Remove(existing);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a custom tag definition by its tag keyword.
        /// </summary>
        public CustomTagDefinition GetByTag(string tag)
        {
            return customTags.FirstOrDefault(t =>
                string.Equals(t.Tag, tag, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all enabled custom tags.
        /// </summary>
        public IEnumerable<CustomTagDefinition> GetEnabledTags()
        {
            return customTags.Where(t => t.Enabled);
        }

        /// <summary>
        /// Loads custom tags from a serialized list.
        /// </summary>
        public void LoadFrom(IEnumerable<CustomTagDefinition> definitions)
        {
            customTags.Clear();
            if (definitions != null)
            {
                foreach (var def in definitions)
                    customTags.Add(def);
            }
        }

        /// <summary>
        /// Returns a copy of the current custom tags for serialization.
        /// </summary>
        public List<CustomTagDefinition> ToList()
        {
            return customTags.ToList();
        }
    }
}
