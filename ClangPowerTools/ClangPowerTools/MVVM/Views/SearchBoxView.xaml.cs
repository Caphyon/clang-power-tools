using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for SearchBoxView.xaml
  /// </summary>
  public partial class SearchBoxView : UserControl
  {
    #region Cosntructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public SearchBoxView()
    {
      InitializeComponent();
      DataContext = SettingsViewModelProvider.TidyChecksViewModel;
    }

    #endregion
  }
}
