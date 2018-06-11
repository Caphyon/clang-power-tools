using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for SearchBox.xaml
  /// </summary>
  public partial class SearchBox : UserControl //, INotifyPropertyChanged
  {
    #region Cosntructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public SearchBox()
    {
      InitializeComponent();
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// When the SearchBox got focus select the TextBox to make it's border blue
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
      var t = (TextBox)sender;
      t.SelectAll();
    }


    /// <summary>
    /// When the SearchBox got mouse capture select the TextBox to make it's border blue
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchTextBox_GotMouseCapture(object sender, MouseEventArgs e)
    {
      var t = (TextBox)sender;
      t.SelectAll();
    }


    /// <summary>
    /// Focus the SearchBox or clear all the text from it
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Search_Click(object sender, RoutedEventArgs e)
    {
      // If the SearchBox is not empty then the Clear Search option was pressed 
      // Make the SearchBox empty
      if (false == string.IsNullOrWhiteSpace(SearchTextBox.Text))
        SearchTextBox.Text = string.Empty;

      // Put the mouse cursor inside the SearchBox by focus it
      SearchTextBox.Focus();
    }

    #endregion

  }
}
