using ClangPowerTools;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
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

      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        files.Add(new FileModel { FileName = path.Name });
      }
      Files = files;
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
