using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DiffWindow.xaml
  /// </summary>
  public partial class DiffWindow : Window
  {
    #region Members

    private readonly DiffViewModel diffViewModel;

    #endregion

    #region Constructor
    public DiffWindow()
    {
      InitializeComponent();
      diffViewModel = new DiffViewModel(this);
      DataContext = diffViewModel;
    }

    #endregion

    #region Public Methods

    public async Task ShowDiffAsync(List<string> filePaths)
    {
      await diffViewModel.DiffDocumentsAsync(filePaths);
    }

    #endregion

    #region Private Methods

    private void Diff_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.VerticalChange == 0 && e.HorizontalChange == 0) return;
      if (sender == DiffInput)
      {
        DiffOutput.ScrollToVerticalOffset(e.VerticalOffset);
        DiffOutput.ScrollToHorizontalOffset(e.HorizontalOffset);
      }
      else
      {
        DiffInput.ScrollToVerticalOffset(e.VerticalOffset);
        DiffInput.ScrollToHorizontalOffset(e.HorizontalOffset);
      }
    }

    //TODO try to bind to ViewModel
    private void InputText_TextChanged(object sender, TextChangedEventArgs e)
    {

    }

    private void BooleanCombobox_DropDownClosed(object sender, System.EventArgs e)
    {

    }

    private void OpenMultipleInput(object sender, RoutedEventArgs e)
    {

    }

    #endregion
  }
}
