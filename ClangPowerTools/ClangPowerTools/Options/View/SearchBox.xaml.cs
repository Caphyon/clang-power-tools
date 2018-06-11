using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for SearchBox.xaml
  /// </summary>
  public partial class SearchBox : UserControl, INotifyPropertyChanged
  {

    private string mSearchtext = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    public string SearchText
    {
      get { return mSearchtext; }
      set
      {
        mSearchtext = value;
        OnPropertyChanged("SearchText");
      }
    }

    public SearchBox()
    {
      InitializeComponent();
    }

    private void OnPropertyChanged(string aPropertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(aPropertyName));
    }


    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
      var t = (TextBox)sender;
      t.SelectAll();
    }

    private void SearchTextBox_GotMouseCapture(object sender, MouseEventArgs e)
    {
      var t = (TextBox)sender;
      t.SelectAll();
    }


    private void Search_Click(object sender, RoutedEventArgs e)
    {
      if (false == string.IsNullOrWhiteSpace(SearchTextBox.Text))
        SearchTextBox.Text = string.Empty;

      SearchTextBox.Focus();
    }

  }
}
