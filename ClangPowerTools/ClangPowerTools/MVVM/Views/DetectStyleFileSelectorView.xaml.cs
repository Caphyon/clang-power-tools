using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectFormatStyleMenuView.xaml
  /// </summary>
  public partial class DetectStyleFileSelectorView : Window
  {
    private readonly DetectStyleFileSelectorViewModel fileSelectorViewModel;

    public DetectStyleFileSelectorView()
    {
      InitializeComponent();
      fileSelectorViewModel = new DetectStyleFileSelectorViewModel(this);
      DataContext = fileSelectorViewModel;
    }

    private void RemoveFileButton_Click(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      ((DetectStyleFileSelectorViewModel)DataContext).RemoveFile(elementIndex);
    }

    private int GetElementIndex(FrameworkElement frameworkElement)
    {
      var element = frameworkElement.DataContext;
      return CollectionItems.Items.IndexOf(element);
    }

  }
}