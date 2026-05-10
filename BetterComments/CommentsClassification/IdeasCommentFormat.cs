using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.CommentsClassification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.IdeasComment)]
    [Name(Constants.IdeasComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class IdeasCommentFormat : ClassificationFormatDefinition
    {
        public IdeasCommentFormat()
        {
            this.DisplayName = "Better Comments - Ideas";
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#1AA9F5");
        }
    }
}
