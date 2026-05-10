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

            criticalBold = true;
            criticalUnderline = false;
            criticalHighlightKeywordOnly = false;

            warningBold = false;
            warningUnderline = false;
            warningHighlightKeywordOnly = false;

            ideasBold = false;
            ideasUnderline = false;
            ideasHighlightKeywordOnly = false;

            infoBold = false;
            infoUnderline = false;
            infoHighlightKeywordOnly = false;

            // Load saved settings from VS store if they exist
            SettingsStore.LoadSettings(this);
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

        // Critical Settings
        private bool criticalBold;
        [Setting]
        public bool CriticalBold
        {
            get => criticalBold;
            set => SetField(ref criticalBold, value);
        }

        private bool criticalUnderline;
        [Setting]
        public bool CriticalUnderline
        {
            get => criticalUnderline;
            set => SetField(ref criticalUnderline, value);
        }

        private bool criticalHighlightKeywordOnly;
        [Setting]
        public bool CriticalHighlightKeywordOnly
        {
            get => criticalHighlightKeywordOnly;
            set => SetField(ref criticalHighlightKeywordOnly, value);
        }

        // Warning Settings
        private bool warningBold;
        [Setting]
        public bool WarningBold
        {
            get => warningBold;
            set => SetField(ref warningBold, value);
        }

        private bool warningUnderline;
        [Setting]
        public bool WarningUnderline
        {
            get => warningUnderline;
            set => SetField(ref warningUnderline, value);
        }

        private bool warningHighlightKeywordOnly;
        [Setting]
        public bool WarningHighlightKeywordOnly
        {
            get => warningHighlightKeywordOnly;
            set => SetField(ref warningHighlightKeywordOnly, value);
        }

        // Ideas Settings
        private bool ideasBold;
        [Setting]
        public bool IdeasBold
        {
            get => ideasBold;
            set => SetField(ref ideasBold, value);
        }

        private bool ideasUnderline;
        [Setting]
        public bool IdeasUnderline
        {
            get => ideasUnderline;
            set => SetField(ref ideasUnderline, value);
        }

        private bool ideasHighlightKeywordOnly;
        [Setting]
        public bool IdeasHighlightKeywordOnly
        {
            get => ideasHighlightKeywordOnly;
            set => SetField(ref ideasHighlightKeywordOnly, value);
        }

        // Info Settings
        private bool infoBold;
        [Setting]
        public bool InfoBold
        {
            get => infoBold;
            set => SetField(ref infoBold, value);
        }

        private bool infoUnderline;
        [Setting]
        public bool InfoUnderline
        {
            get => infoUnderline;
            set => SetField(ref infoUnderline, value);
        }

        private bool infoHighlightKeywordOnly;
        [Setting]
        public bool InfoHighlightKeywordOnly
        {
            get => infoHighlightKeywordOnly;
            set => SetField(ref infoHighlightKeywordOnly, value);
        }
    }
}
