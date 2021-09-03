using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
    public class FolderExplorerViewModel
    {
    #region Members
    private string folderPath = string.Empty;
    private FolderExplorer folderExplorerView;

    private ICommand findFolderPathCommand;
  //private ICommand folderTextPathCommand;
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

    //public ICommand InputToAdd
    //{
    //  get => folderTextPathCommand ?? (folderTextPathCommand = new RelayCommand(() => GetFolderPath(), () => CanExecute));
    //}
    #endregion

    #region Public Methods
    public void GetFolderPath()
    {
      //Implement get folder path
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
