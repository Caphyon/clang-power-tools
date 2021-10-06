using ClangPowerTools;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
      itemsCollector.CollectSelectedItems();
      foreach (var item in itemsCollector.Items)
      {
        files.Add(new FileModel { FileName = item.GetName() });
      }
      files.Add(new FileModel { FileName = "just a test" });
      Files = files;
    }

    #region Properties

    public ObservableCollection<FileModel> Files { get; set; } = new ObservableCollection<FileModel>();

    public void UpdateViewModel()
    {
      ItemsCollector itemsCollectora = new ItemsCollector();
      itemsCollectora.CollectSelectedItems();

      FilePathCollector fileCollector = new FilePathCollector();
      var filesPath = fileCollector.Collect(itemsCollectora.Items).ToList();
      foreach (var item in filesPath)
      {
        files.Add(new FileModel { FileName = item });
      }
      files.Add(new FileModel { FileName = "show selected files method executed" });
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
