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
    public event PropertyChangedEventHandler PropertyChanged;
    private ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private TidyToolWindowView tidyToolWindowView;
    private ItemsCollector itemsCollector = new ItemsCollector();
    private ICommand showFiles;


    public TidyToolWindowViewModel(TidyToolWindowView tidyToolWindowView)
    {

      this.tidyToolWindowView = tidyToolWindowView;
    }

    #region Properties

    public ObservableCollection<FileModel> Files { get; set; } = new ObservableCollection<FileModel>();

    public void UpdateViewModel(List<string> filesPath)
    {

      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        files.Add(new FileModel { FileName = path.Name });
      }
      files.Add(new FileModel { FileName = "------------------------------" });
      Files = files;
    }

    public void DisplayFiles()
    {
      itemsCollector.CollectSelectedItems();
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion

    public ICommand ShowFiles
    {
      get => showFiles ??= new RelayCommand(() => ShowSelectedFiles(), () => CanExecute);
    }


    private void ShowSelectedFiles()
    {
      itemsCollector.CollectSelectedItems();
      foreach (var item in itemsCollector.Items)
      {
        files.Add(new FileModel { FileName = item.GetName() });
      }
      files.Add(new FileModel { FileName = "show selected files method executed" });
      Files = files;
    }
  }
}
