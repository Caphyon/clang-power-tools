using ClangPowerTools.MVVM.Models;
using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for InputList.xaml
  /// </summary>
  public partial class FolderExplorer : Window
  {
    private readonly FolderExplorerViewModel viewModel;

    #region Constructor
    public FolderExplorer()
    {
      InitializeComponent();
      viewModel= new FolderExplorerViewModel(this);
      DataContext = viewModel;
      Owner = SettingsProvider.SettingsView;
    }
    #endregion
  }
}
