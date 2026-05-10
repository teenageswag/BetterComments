using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.CommentsClassification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.CriticalComment)]
    [Name(Constants.CriticalComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class CriticalCommentFormat : ClassificationFormatDefinition
    {
        public CriticalCommentFormat()
        {
            this.DisplayName = "Better Comments - Critical";
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#FF2A3D");
        }
    }
}
