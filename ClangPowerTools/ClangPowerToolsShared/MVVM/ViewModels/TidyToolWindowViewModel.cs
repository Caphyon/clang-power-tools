using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerTools.Views;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models;
using ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels;
using ClangPowerToolsShared.MVVM.ViewModels.ToolWindow;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class TidyToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private TidyToolWindowView tidyToolWindowView;
    private MessageModel messageModel;
    private string listVisibility = UIElementsConstants.Visibile;
    private TidyToolWindowController TidyController;
    //To not refresh files value every time (with the same files), and to not refresh check box value
    bool filesAlreadyExists = false;
    bool wasMadeTidyOnFiles = false;

    private ICommand tidyAllCommand;
    private ICommand fixAllCommand;
    private ICommand discardAllCommand;
    private ICommand removeAllCommand;

    public ObservableCollection<FileModel> Files { get { return TidyController.files; }
      set
      {
        TidyController.files = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Files"));
      }
    }

    #endregion

    #region Properties

    public TidyToolWindowModel TidyToolWindowModel
    {
      get { return TidyController.tidyToolWindowModel; }
      set
      {
        TidyController.tidyToolWindowModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyToolWindowModel"));
      }
    }

    public MessageModel MessageModel
    {
      get { return messageModel; }
      set
      {
        messageModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MessageModel"));
      }
    }

    public string ListVisibility
    {
      get { return listVisibility; }
      set
      {
        listVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListVisibility"));
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region Commands

    public ICommand TidyAllCommand
    {
      get => tidyAllCommand ?? (tidyAllCommand = new RelayCommand(() => TidyAllFilesAsync().SafeFireAndForget(), () => CanExecute));
    }

    public ICommand FixAllCommand
    {
      get => fixAllCommand ?? (fixAllCommand = new RelayCommand(() => FixAllFilesAsync().SafeFireAndForget(), () => CanExecute));
    }

    public ICommand DiscardAllCommand
    {
      get => discardAllCommand ?? (discardAllCommand = new RelayCommand(() => DiscardAllFiles(), () => CanExecute));
    }

    public ICommand RemoveAllCommand
    {
      get => removeAllCommand ?? (removeAllCommand = new RelayCommand(() => RemoveAllFiles(), () => CanExecute));
    }

    #endregion

    #region Constructos

    public TidyToolWindowViewModel(TidyToolWindowView tidyToolWindowView)
    {
      VSColorTheme.ThemeChanged += ThemeChangeEvent;
      this.tidyToolWindowView = tidyToolWindowView;
      //init
      TidyController = new TidyToolWindowController();
      messageModel = new MessageModel();

      TidyController.tidyToolWindowModel.ButtonVisibility = UIElementsConstants.Visibile;
      TidyController.tidyToolWindowModel.ProgressBarVisibility = UIElementsConstants.Hidden;

      //Create groups
      CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Files);
      PropertyGroupDescription groupDescription = new PropertyGroupDescription("FilesType");

      view.GroupDescriptions.Add(groupDescription);
    }

    #endregion

    #region Public Methods

    public void OpenTidyToolWindow(List<string> filesPath)
    {
      RefreshOnWindowUpdate();
      TidyController.InitTidyToolWindow(filesPath);
      wasMadeTidyOnFiles = true;
      filesAlreadyExists = false;
    }

    public void UpdateViewModel(List<string> filesPath)
    {
      //if tidy fix was made
      if (!wasMadeTidyOnFiles)
      {
        TidyController.AddHeadersInFilesList(filesPath);
      }

      if (!filesAlreadyExists)
      {
        TidyController.AddFilesInFilesList(filesPath);
        filesAlreadyExists = true;
      }
      if (!Directory.Exists(TidyConstants.TempsFolderPath))
        Directory.CreateDirectory(TidyConstants.TempsFolderPath);
    }

    private async Task TidyAllFilesAsync(List<string> paths = null)
    {
      wasMadeTidyOnFiles = true;
      TidyController.TidyFilesAsync(paths);
    }


    public void CheckOrUncheckAll()
    {
      TidyController.CheckOrUncheckAll();
    }

    public async Task FixAllFilesAsync(FileModel file = null)
    {
      TidyController.FixAllFilesAsync(file);
      wasMadeTidyOnFiles = false;
      if (file is not null)
      {
        DiffFile(file);
      }
    }

    /// <summary>
    /// Update checked numer on check and uncheck action
    /// </summary>
    /// <param name="file"></param>
    public void UpdateCheckedNumber(FileModel file)
    {
      TidyController.UpdateCheckedNumber(file);
      TidyToolWindowModel = TidyController.tidyToolWindowModel;
    }

    public void DiffFile(FileModel file)
    {
      TidyController.DiffBetweenCopyAndCurrent(file);
    }

    #endregion

    #region Private Method

    private void RemoveAllFiles()
    {
      TidyController.BeforeCommand();
      TidyController.RemoveFiles();
      //Display a message if no file in list
      if (Files.Count == 0)
      {
        listVisibility = UIElementsConstants.Hidden;
        ListVisibility = ListVisibility;
        messageModel.Visibility = UIElementsConstants.Visibile;
        messageModel.TextMessage = "You don't have any files, run tidy to add files";
        MessageModel = messageModel;
      }
      TidyController.AfterCommand();
    }

    private void DiscardAllFiles()
    {
      if (TidyToolWindowModel.CountFilesModel.TotalCheckedFixedFiles != 0)
      {
        TidyController.BeforeCommand();
        var checkedFiles = TidyController.files.Where(f => f.IsChecked).ToList();
        foreach (var file in checkedFiles)
        {
          if (file.IsChecked)
          {
            TidyController.DiscardFile(file);
            //If is a header remove from file list
            if(file.FilesType == FileType.Header)
            {
              TidyController.RemoveFiles(file);
            }
          }
        }
        TidyController.AfterCommand();
      }
    }

    public void ThemeChangeEvent(ThemeChangedEventArgs e)
    {
      TidyController.tidyToolWindowModel.ChangeIconsTheme();
      foreach (var file in TidyController.files)
      {
        file.ChangeIconsTheme();
      }
    }

    /// <summary>
    /// Make list visible after tidy from toolbar or contextMenu
    /// </summary>
    private void RefreshOnWindowUpdate()
    {
      listVisibility = UIElementsConstants.Visibile;
      messageModel.Visibility = UIElementsConstants.Hidden;
      ListVisibility = listVisibility;
      MessageModel = messageModel;
    }

    #endregion
  }
}