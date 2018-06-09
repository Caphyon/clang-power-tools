using System;
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

    private string mSearchtext = "";

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

    //private static DependencyProperty SearchTextProperty =
    //  DependencyProperty.Register("SearchText", typeof(string), typeof(SearchBox),
    //    new FrameworkPropertyMetadata(string.Empty, 
    //      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Inherits));

    //public string SearchText
    //{
    //  get { return (string)GetValue(SearchTextProperty); }
    //  set
    //  {
    //    SetValue(SearchTextProperty, value);
    //  }
    //}

    public SearchBox()
    {
      InitializeComponent();
      //DataContext = this;
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
      // If the SearchBox is empty do nothing
      // If the SearchBox is not empty delete all the tet inside
      //
      // Write the code here
      //
    }

  }
}
