using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.CommentsClassification
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.InfoComment)]
    [Name(Constants.InfoComment)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class InfoCommentFormat : ClassificationFormatDefinition
    {
        public InfoCommentFormat()
        {
            this.DisplayName = "Better Comments - Info";
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#6FEA2D");
        }
    }
}
