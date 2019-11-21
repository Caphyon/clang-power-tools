using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ClangPowerTools.Squiggle
{
  /// <summary>
  /// Classifier provider. It adds the classifier to the set of classifiers.
  /// </summary>
  [Export(typeof(IClassifierProvider))]
  [ContentType("text")] // This classifier applies to all text files.
  internal class EditorClassifier1Provider : IClassifierProvider
  {
    // Disable "Field is never assigned to..." compiler's warning. Justification: the field is assigned by MEF.
#pragma warning disable 649

    /// <summary>
    /// Classification registry to be used for getting a reference
    /// to the custom classification type later.
    /// </summary>
    [Import]
    private IClassificationTypeRegistryService classificationRegistry;

#pragma warning restore 649

    #region IClassifierProvider

    /// <summary>
    /// Gets a classifier for the given text buffer.
    /// </summary>
    /// <param name="buffer">The <see cref="ITextBuffer"/> to classify.</param>
    /// <returns>A classifier for the text buffer, or null if the provider cannot do so in its current state.</returns>
    public IClassifier GetClassifier(ITextBuffer buffer)
    {
      return buffer.Properties.GetOrCreateSingletonProperty<EditorClassifier1>(creator: () => new EditorClassifier1(this.classificationRegistry));
    }

    #endregion
  }
}
