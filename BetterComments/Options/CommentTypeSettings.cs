using System.ComponentModel;

namespace BetterComments.Options
{
    /// <summary>
    /// Settings for a specific comment type (Critical, Warning, Ideas, Info).
    /// </summary>
    public class CommentTypeSettings : PropertyChangeNotifier
    {
        private bool bold;
        public bool Bold
        {
            get => bold;
            set => SetField(ref bold, value);
        }

        private bool underline;
        public bool Underline
        {
            get => underline;
            set => SetField(ref underline, value);
        }

        private bool highlightKeywordOnly;
        public bool HighlightKeywordOnly
        {
            get => highlightKeywordOnly;
            set => SetField(ref highlightKeywordOnly, value);
        }

        public CommentTypeSettings(bool defaultBold = false, bool defaultUnderline = false, bool defaultHighlightKeywordOnly = false)
        {
            bold = defaultBold;
            underline = defaultUnderline;
            highlightKeywordOnly = defaultHighlightKeywordOnly;
        }
    }
}
