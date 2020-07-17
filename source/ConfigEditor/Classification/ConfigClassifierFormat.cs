using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ConfigEditor.Classification
{
    /// <summary>
    /// Defines an editor format for the ConfigClassifier type that has a purple background
    /// and is underlined.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ConfigClassifier")]
    [Name("ConfigClassifier")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class ConfigClassifierFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigClassifierFormat"/> class.
        /// </summary>
        public ConfigClassifierFormat()
        {
            this.DisplayName = "ConfigClassifier"; // Human readable version of the name
            this.BackgroundColor = Colors.BlueViolet;
            this.TextDecorations = System.Windows.TextDecorations.Underline;
        }
    }

    /// <summary>
    /// Defines an editor format for the ConfigClassifier type that has a purple background
    /// and is underlined.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ConfigKeyWord")]
    [Name("ConfigKeyWord")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class ConfigKeywordClassifierFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigClassifierFormat"/> class.
        /// </summary>
        public ConfigKeywordClassifierFormat()
        {
            this.DisplayName = "ConfigKeyWord"; // Human readable version of the name
            this.ForegroundColor = Colors.DodgerBlue;
        }
    }


    /// <summary>
    /// Defines an editor format for the ConfigClassifier type that has a purple background
    /// and is underlined.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ConfigOperator")]
    [Name("ConfigOperator")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class ConfigOperatorClassifierFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigClassifierFormat"/> class.
        /// </summary>
        public ConfigOperatorClassifierFormat()
        {
            this.DisplayName = "ConfigStruct"; // Human readable version of the name
            this.ForegroundColor = Colors.Green;
        }
    }

    /// <summary>
    /// Defines an editor format for the ConfigClassifier type that has a purple background
    /// and is underlined.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ConfigComment")]
    [Name("ConfigComment")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class ConfigCommentClassifierFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigClassifierFormat"/> class.
        /// </summary>
        public ConfigCommentClassifierFormat()
        {
            this.DisplayName = "ConfigComment"; // Human readable version of the name
            this.ForegroundColor = Colors.LimeGreen;
        }
    }

    /// <summary>
    /// Defines an editor format for the ConfigClassifier type that has a purple background
    /// and is underlined.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ConfigStruct")]
    [Name("ConfigStruct")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class ConfigStructClassifierFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigClassifierFormat"/> class.
        /// </summary>
        public ConfigStructClassifierFormat()
        {
            this.DisplayName = "ConfigStruct"; // Human readable version of the name
            this.ForegroundColor = Colors.Turquoise;
        }
    }
}
