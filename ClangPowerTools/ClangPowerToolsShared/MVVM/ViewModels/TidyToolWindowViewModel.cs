using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerTools.Views;
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

    #endregion

    public TidyToolWindowViewModel(TidyToolWindowView tidyToolWindowView)
    {
      Files = files;
      this.tidyToolWindowView = tidyToolWindowView;
    }

    public ObservableCollection<FileModel> Files { get; set; } = new ObservableCollection<FileModel>();

    public void UpdateViewModel(List<string> filesPath)
    {
      files.Clear();
      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        files.Add(new FileModel { FileName = path.Name });
      }
      Files = files;
      //copy files in temp folder
      string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "Temp");
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
