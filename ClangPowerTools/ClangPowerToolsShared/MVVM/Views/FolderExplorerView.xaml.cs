using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for InputList.xaml
  /// </summary>
  public partial class FolderExplorerView : Window
  {
    #region Constructor

    public FolderExplorerView(List<LlvmModel> llvms, ObservableCollection<string> installedLlvms)
    {
      InitializeComponent();
      DataContext = new FolderExplorerViewModel(this, llvms, installedLlvms);
      Owner = SettingsProvider.SettingsView;
    }

    #endregion
  }
}
