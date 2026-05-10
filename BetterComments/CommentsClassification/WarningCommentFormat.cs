using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.CommentsClassification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.WarningComment)]
    [Name(Constants.WarningComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class WarningCommentFormat : ClassificationFormatDefinition
    {
        public WarningCommentFormat()
        {
            this.DisplayName = "Better Comments - Warning";
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#FFAA33");
        }
    }
}
