using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Views;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class FolderExplorerViewModel
  {
    #region Members
    private FolderExplorer folderExplorerView;

    private ICommand findFolderPathCommand;
    #endregion

    #region Contructor 
    public FolderExplorerViewModel(FolderExplorer view)
    {
      folderExplorerView = view;
    }
    #endregion

    #region Commands
    public ICommand FindFolderPathCommand
    {
      get => findFolderPathCommand ?? (findFolderPathCommand = new RelayCommand(() => GetFolderPath(), () => CanExecute));
    }

    #endregion

    #region Public Methods
    public void GetFolderPath()
    {
      //Implement get folder path
      var text = folderExplorerView.FolderPathTextBox.Text;
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }
    #endregion
  }
}
