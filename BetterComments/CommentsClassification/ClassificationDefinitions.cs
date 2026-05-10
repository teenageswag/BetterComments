using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.CommentsClassification
{
    internal static class ClassificationDefinitions
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.CriticalComment)]
        internal static ClassificationTypeDefinition CriticalDefinition = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.WarningComment)]
        internal static ClassificationTypeDefinition WarningDefinition = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.IdeasComment)]
        internal static ClassificationTypeDefinition IdeasDefinition = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.InfoComment)]
        internal static ClassificationTypeDefinition InfoDefinition = null;
    }
}
