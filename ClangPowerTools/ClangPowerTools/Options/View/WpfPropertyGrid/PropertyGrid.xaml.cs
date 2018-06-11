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


    #region Public Members


    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// The view modes from which the user can choose the mode 
    /// he want to see the options from the current option page
    /// </summary>
    public enum ViewModes
    {
      Grouped,
      Alphabetical
    }


    #endregion


    #region Private members

    /// <summary>
    /// Stores the SearchBox content
    /// </summary>
    private string mSearchtext = "";


    /// <summary>
    /// Object for that the pairs (property name, property value)
    /// are displayed
    /// </summary>
    private object mSelectedObject = null;

    /// <summary>
    /// The view mode choose by user to see the options from an option page
    /// It can be Grouped or Alphabetical
    /// </summary>
    private ViewModes mViewMode;

    /// <summary>
    /// Properties for the selected object
    /// </summary>
    private PropertyCollection mProperties = null;


    /// <summary>
    /// Define the format of the properties in the Property Grid
    /// </summary>
    private const string GridHelpFormat =
      "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TextWrapping=\"Wrap\" FontFamily=\"Segoe UI\" FontSize=\"12\">{0}</TextBlock>";


    #endregion


    #region Dependency Properties Memberes

    /// <summary>
    /// Dependency property member for displaying the Categorized and Alphabetical properties sorting
    /// </summary>
    public static readonly DependencyProperty ShowGroupingProperty =
      DependencyProperty.Register("ShowGrouping", typeof(bool), typeof(PropertyGrid));

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty ShowFilterProperty =
      DependencyProperty.Register("ShowFilter", typeof(bool), typeof(PropertyGrid));

    /// <summary>
    /// Dependency property member for displaying the SearchBox filter
    /// </summary>
    public static readonly DependencyProperty EnableGroupingProperty =
      DependencyProperty.Register("EnableGrouping", typeof(bool), typeof(PropertyGrid));


    #endregion


    #endregion


    #region Properties


    #region Dependency Properties

    /// <summary>
    /// Dependency property to store the Categorized and Alphabetical displaying option
    /// </summary>
    public bool ShowGrouping
    {
      get { return (bool)GetValue(ShowGroupingProperty); }
      set { SetValue(ShowGroupingProperty, value); }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool EnableGrouping
    {
      get { return (bool)GetValue(EnableGroupingProperty); }
      set { SetValue(EnableGroupingProperty, value); }
    }

    /// <summary>
    /// Dependency property to store the SearchBox displaying option
    /// </summary>
    public bool ShowFilter
    {
      get { return (bool)GetValue(ShowFilterProperty); }
      set { SetValue(ShowFilterProperty, value); }
    }

    #endregion


    #region Public Properties

    /// <summary>
    /// Store the selected view mode of the user
    /// </summary>
    public ViewModes ViewMode
    {
      get { return mViewMode; }
      set
      {
        mViewMode = value;

        // View mode has changed
        // Update the view
        UpdatePropertyView(value);
      }
    }

    /// <summary>
    /// Stores the SearchBox content 
    /// </summary>
    public string SearchText
    {
      get { return mSearchtext; }
      set
      {
        mSearchtext = value;

        // SearchBox content has changed
        NotifyPropertyChanged("SearchText");

        // Apply the option Filter
        FilterPropertyView();
      }
    }

    /// <summary>
    /// Object for that the pairs (property name, property value) are displayed
    /// </summary>
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
          mProperties = new PropertyCollection(mSelectedObject);
        else
          mProperties = null;
        
        // Set the data context
        PropertyList.DataContext = mProperties;

        // Update the view
        UpdatePropertyView(mViewMode);
      }
    }

    #endregion


    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public PropertyGrid()
    {
      InitializeComponent();
      DataContext = this;
      DataContextChanged += GridDataContextChanged;
    }

    #endregion


    #region Methods


    #region Protected Methods

    /// <summary>
    /// Detect when the property has changed
    /// </summary>
    /// <param name="aPropertyName"></param>
    protected void NotifyPropertyChanged(string aPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(aPropertyName));
    }


    #endregion


    #region Private Methods

    /// <summary>
    /// Detect when the data context has changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      SelectedObject = e.NewValue;
    }

    /// <summary>
    /// Detect when the options list has changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Reload the data context
    /// </summary>
    /// <param name="aProperties"></param>
    private void Reload(PropertyCollection aProperties)
    {
      if (null != mProperties)
      {
        PropertyList.DataContext = null;
        PropertyList.DataContext = aProperties;
      }
    }

    /// <summary>
    /// Detect when the categorized button is pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CategorizedButton_Checked(object sender, RoutedEventArgs e)
    {
      // Update the view mode
      ViewMode = ViewModes.Grouped;
    }

    /// <summary>
    /// Detect when the alphabetical button is pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AlphabeticalButton_Checked(object sender, RoutedEventArgs e)
    {
      // Update the view mode
      ViewMode = ViewModes.Alphabetical;
    }


    /// <summary>
    /// Apply the new filter property to the current view and option list
    /// </summary>
    private void FilterPropertyView()
    {
      // Get the view
      System.ComponentModel.ICollectionView crtView = CollectionViewSource.GetDefaultView(mProperties);
      if (crtView == null)
        return;

      // if the SearchBox is not empty apply the filter
      if (false == string.IsNullOrWhiteSpace(SearchText))
        crtView.Filter = FilerOption;
      else
        crtView.Filter = null;
    }

    /// <summary>
    /// Check which view mode is selected and update the current view 
    /// </summary>
    /// <param name="aViewMode"></param>
    private void UpdatePropertyView(ViewModes aViewMode)
    {
      // Get the view
      System.ComponentModel.ICollectionView crtView = CollectionViewSource.GetDefaultView(mProperties);
      if (crtView == null)
        return;

      if (aViewMode == ViewModes.Alphabetical)
      {
        // Remove the groups
        crtView.GroupDescriptions.Clear();

        // Sort options alphabetically by name
        crtView.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
      }
      else if (aViewMode == ViewModes.Grouped)
      {
        // Remove the sorting
        crtView.SortDescriptions.Clear();

        // Add the options groups
        crtView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
      }

      crtView.Refresh();
    }

    /// <summary>
    /// Filter option which check each available option if respects the required condition
    /// </summary>
    /// <param name="propertyData"></param>
    /// <returns></returns>
    private bool FilerOption(object propertyData)
    {
      if (!(propertyData is PropertyData))
        return false;

      // Check if the option name contains the SearchBox content
      return (propertyData as PropertyData).DisplayName.Contains(SearchText);
    }

    #endregion


    #endregion

  }
}
