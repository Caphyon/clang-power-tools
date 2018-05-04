using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for PropertyGrid.xaml
  /// </summary>
  public partial class PropertyGrid : UserControl, INotifyPropertyChanged
  {




    #region Private members
    /// <summary>
    /// Object for that the pairs (property name, property value)
    /// are displayed
    /// </summary>
    private object mSelectedObject = null;
    /// <summary>
    /// Properties for the selected object
    /// </summary>
    private PropertyCollection mProperties = null;
    #endregion
    public PropertyGrid()
    {
      InitializeComponent();
      DataContextChanged += GridDataContextChanged;
    }

    private void GridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      SelectedObject = e.NewValue;
    }

    public object SelectedObject
    {
      get
      {
        return mSelectedObject;
      }
      set
      {
        mSelectedObject = value;
        if (null != mSelectedObject)
        {
          mProperties = new PropertyCollection(mSelectedObject);
        }
        else
        {
          mProperties = null;
        }
        // Set the data context
        PropertyList.DataContext = mProperties;
        if (null != mProperties)
        {
          // Get the current view of the property collection
          System.ComponentModel.ICollectionView crtView =
            CollectionViewSource.GetDefaultView(mProperties);
          // Create groups using the category property
          crtView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
        }
      }
    }

    private const string GridHelpFormat =
      "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TextWrapping=\"Wrap\" FontFamily=\"Segoe UI\" FontSize=\"12\">{0}</TextBlock>";

    public event PropertyChangedEventHandler PropertyChanged;

    private void PropertyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      // Get the selected element i
      IList addedItems = e.AddedItems;
      if (addedItems.Count > 0)
      {
        PropertyData crtProperty = addedItems[0] as PropertyData;
        if (crtProperty == null)
          return;

        PropNameBlock.Text = crtProperty.DisplayName;
        // Construct the description
        if (!string.IsNullOrWhiteSpace(crtProperty.Description))
        {
          string textBlockString;
          // Remove end of lines from description
          string description = crtProperty.Description.Replace("\r\n", "");
          textBlockString = string.Format(GridHelpFormat, description);
          DescriptionPanel.Content = XamlReader.Parse(textBlockString) as TextBlock;
        }
        else
        {
          // No description
          DescriptionPanel.Content = null;
        }
      }
    }

    private void Reload()
    {
      if (null != mProperties)
      {
        PropertyList.DataContext = null;
        PropertyList.DataContext = mProperties;
      }
    }

    protected void NotifyPropertyChanged(string aPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(aPropertyName));
    }
  }
}
