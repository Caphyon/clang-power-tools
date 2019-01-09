using System.Windows.Controls;
using System.Windows;
using ClangPowerTools;
using ClangPowerTools.Options;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /// <summary>
  /// Choses the data template used to display
  /// a specified property in the property grid
  /// </summary>
  class PropertyTemplateSelector : DataTemplateSelector
  {
    public static bool[] BoolValues = new bool[] { true, false };

    private PropertyGridResources mRes;
    // The property grid that uses the template selector
    private PropertyGrid mGrid;

    public PropertyTemplateSelector()
    {
      mRes = new PropertyGridResources();
      mGrid = null;
    }

    /// <summary>
    /// Select template used to display a property
    /// </summary>
    /// <param name="item">The pair (element, property) that
    /// should be displayed</param>
    /// <param name="container"></param>
    /// <returns></returns>
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (null == mGrid)
      {
        mGrid = VisualTreeHelperExtension.FindAncestor<PropertyGrid>(container);
      }

      PropertyData prop = item as PropertyData;
      string editorName = prop.EditorName;

      if (!string.IsNullOrEmpty(editorName))
        return mRes[editorName] as DataTemplate;

      if (true == prop.HasTextBoxAndBrowseAttribute && !prop.IsReadOnly)
        return mRes["ClangFormatPathDataTemplate"] as DataTemplate;

      if (prop.PropertyType == typeof(ClangFormatSkipValue) && !prop.IsReadOnly)
        return mRes["ClangFormatSkipTemplate"] as DataTemplate;

      if (prop.PropertyType == typeof(string) && !prop.IsReadOnly)
        return mRes["StringDataTemplate"] as DataTemplate;
      
      if (prop.PropertyType == typeof(bool) && !prop.IsReadOnly )
        return mRes["BoolDataTemplate"] as DataTemplate;

      if (prop.PropertyType == typeof(ClangFormatStyle?))
        return mRes["StyleDataTemplate"] as DataTemplate;

      if (prop.PropertyType == typeof(ClangFormatFallbackStyle?))
        return mRes["FallbackStyleDataTemplate"] as DataTemplate;

      if (prop.PropertyType == typeof(ClangGeneralAdditionalIncludes?))
        return mRes["AdditionalIncludesDataTemplate"] as DataTemplate;

      if (prop.PropertyType == typeof(ClangTidyUseChecksFrom?))
        return mRes["UseChecksFromDataTemplate"] as DataTemplate;

      if (prop.PropertyType == typeof(HeaderFiltersValue) && !prop.IsReadOnly)
        return mRes["HeaderFiltersDataTemplate"] as DataTemplate;

      return mRes["DefaultDataTemplate"] as DataTemplate;
    }
  }
}
