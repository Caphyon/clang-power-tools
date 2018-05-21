using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for PropertyGrid.xaml
  /// </summary>
  public partial class PropertyGrid : UserControl, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    public enum ViewModes
    {
      Grouped,
      Alphabetical
    }

    #region Private members

    /// <summary>
    /// Object for that the pairs (property name, property value)
    /// are displayed
    /// </summary>
    private object mSelectedObject = null;
    private ViewModes mViewMode;

    /// <summary>
    /// Properties for the selected object
    /// </summary>
    private PropertyCollection mProperties = null;

    private const string GridHelpFormat =
      "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TextWrapping=\"Wrap\" FontFamily=\"Segoe UI\" FontSize=\"12\">{0}</TextBlock>";


    #endregion


    #region Dependency Properties Memberes

    public static readonly DependencyProperty ShowGroupingProperty =
      DependencyProperty.Register("ShowGrouping", typeof(bool), typeof(PropertyGrid));

    public static readonly DependencyProperty ShowFilterProperty =
      DependencyProperty.Register("ShowFilter", typeof(bool), typeof(PropertyGrid));

    public static readonly DependencyProperty EnableGroupingProperty =
      DependencyProperty.Register("EnableGrouping", typeof(bool), typeof(PropertyGrid));

    #endregion


    #endregion

    #region Properties


    #region Dependency Properties


    public bool ShowGrouping
    {
      get { return (bool)GetValue(ShowGroupingProperty); }
      set { SetValue(ShowGroupingProperty, value); }
    }

    public bool ShowFilter
    {
      get { return (bool)GetValue(ShowFilterProperty); }
      set { SetValue(ShowFilterProperty, value); }
    }
    public bool EnableGrouping
    {
      get { return (bool)GetValue(EnableGroupingProperty); }
      set { SetValue(EnableGroupingProperty, value); }
    }


    #endregion


    public ViewModes ViewMode
    {
      get { return mViewMode; }
      set
      {
        mViewMode = value;
        UpdatePropertyView(value);
      }
    }

    #endregion


    #region Public Methods


    public PropertyGrid()
    {
      InitializeComponent();

      DataContext = this;

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
        UpdatePropertyView(mViewMode);
      }
    }


    #endregion


    #region Protected Methods


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
          DescriptionPanel.Text = description; //XamlReader.Parse(textBlockString) as TextBlock;
        }
        else
        {
          // No description
          DescriptionPanel.Text = null;
        }
      }
    }

    private void Reload(PropertyCollection aProperties)
    {
      if (null != mProperties)
      {
        PropertyList.DataContext = null;
        PropertyList.DataContext = aProperties;
      }
    }

    protected void NotifyPropertyChanged(string aPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(aPropertyName));
    }

    private void CategorizedButton_Checked(object sender, RoutedEventArgs e)
    {
      ViewMode = ViewModes.Grouped;
    }

    private void AlphabeticalButton_Checked(object sender, RoutedEventArgs e)
    {
      ViewMode = ViewModes.Alphabetical;
    }

    private void UpdatePropertyView(ViewModes aViewMode)
    {
      System.ComponentModel.ICollectionView crtView = CollectionViewSource.GetDefaultView(mProperties);
      if (crtView == null)
        return;

      if (aViewMode == ViewModes.Alphabetical)
      {
        crtView.GroupDescriptions.Clear();
        crtView.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
      }
      else if (aViewMode == ViewModes.Grouped)
      {
        crtView.SortDescriptions.Clear();
        crtView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
      }

      crtView.Refresh();
    }

    #endregion

  }
}
