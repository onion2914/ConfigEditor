using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ConfigEditor.Classification
{
    /// <summary>
    /// Classification type definition export for ConfigClassifier
    /// </summary>
    internal static class ConfigClassifierClassificationDefinition
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        /// <summary>
        /// Defines the "ConfigClassifier" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ConfigClassifier")]
        private static ClassificationTypeDefinition typeDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ConfigKeyWord")]
        private static ClassificationTypeDefinition configKeyWord;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ConfigOperator")]
        private static ClassificationTypeDefinition configOperator;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ConfigComment")]
        private static ClassificationTypeDefinition configComment;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ConfigStruct")]
        private static ClassificationTypeDefinition configStruct;

#pragma warning restore 169
    }
}
