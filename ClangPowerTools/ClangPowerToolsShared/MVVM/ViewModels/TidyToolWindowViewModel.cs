using ClangPowerTools;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class TidyToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private TidyToolWindowView tidyToolWindowView;
    private ItemsCollector itemsCollector = new ItemsCollector();
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

    public void DisplayFiles()
    {
      itemsCollector.CollectSelectedItems();
    }
    #endregion
  }
}
