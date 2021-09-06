using ClangPowerToolsShared.MVVM.ViewModels;
using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for InputList.xaml
  /// </summary>
  public partial class FolderExplorer : Window
  {
    #region Constructor
    public FolderExplorer()
    {
      InitializeComponent();
      DataContext = new FolderExplorerViewModel(this);
      Owner = SettingsProvider.SettingsView;
    }
    #endregion

    public void ResetView()
    {
      DataContext = new FolderExplorerViewModel(this);
    }
  }
}
