using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerTools.SilentFile;
using ClangPowerTools.Views;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class TidyToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Properties

    public event PropertyChangedEventHandler PropertyChanged;
    private ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private TidyToolWindowView tidyToolWindowView;
    private ItemsCollector itemsCollector = new ItemsCollector();
    private ICommand showFiles;
    private ICommand tidyAllCommand;
    private ICommand fixAllCommand;
    private ICommand discardAllCommand;
    private ICommand removeAllCommand;
    private readonly string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "Temp");

    #endregion

    #region Commands
    public ICommand TidyAllCommand
    {
      get => tidyAllCommand ?? (tidyAllCommand = new RelayCommand(() => TidyAllFiles(), () => CanExecute));
    }

    public ICommand FixAllCommand
    {
      get => fixAllCommand ?? (fixAllCommand = new RelayCommand(() => FixAllFiles(), () => CanExecute));
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

    public TidyToolWindowViewModel(TidyToolWindowView tidyToolWindowView)
    {
      Files = files;
      this.tidyToolWindowView = tidyToolWindowView;
    }

    public ObservableCollection<FileModel> Files { get; set; } = new ObservableCollection<FileModel>();

    public void DiscardFile(string path)
    {
      FileInfo file = new(path);
      using var silentFileController = new SilentFileChangerController();
      using var fileChangerWatcher = new FileChangerWatcher();

      var dte2 = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      string solutionFolderPath = SolutionInfo.IsOpenFolderModeActive() ?
        dte2.Solution.FullName : dte2.Solution.FullName
                                  .Substring(0, dte2.Solution.FullName.LastIndexOf('\\'));
      fileChangerWatcher.Run(solutionFolderPath);

      var copyFile = Path.Combine(tempFolderPath, file.Name);
      if(File.Exists(copyFile))
      {
        File.Copy(copyFile, file.FullName, true);
        File.Delete(copyFile);
      }
    }

    public void DiscardAllFiles()
    {

    }

    public void TidyAllFiles()
    {

    }

    public void RemoveAllFiles()
    {

    }

    public void FixAllFiles()
    {

    }

    public void UpdateViewModel(List<string> filesPath)
    {
      files.Clear();
      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        files.Add(new FileModel { FileName = path.Name, FullFileName = path.FullName });
      }
      Files = files;
      //copy files in temp folder
      if (Directory.Exists(tempFolderPath))
        Directory.Delete(tempFolderPath, true);
      Directory.CreateDirectory(tempFolderPath);
      if (Directory.Exists(tempFolderPath))
      {
        foreach (string path in filesPath)
        {
          FileInfo file = new(path);
          var copyFile = Path.Combine(tempFolderPath, file.Name);
          File.Copy(file.FullName, copyFile, true);
        }
      }

      TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, CommandUILocation.ContextMenu, null, filesPath);
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

  }
}
