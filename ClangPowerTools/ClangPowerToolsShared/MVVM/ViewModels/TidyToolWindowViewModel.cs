using ClangPowerTools;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class TidyToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private List<FileModel> files = new List<FileModel> { new FileModel { FileName="testtttt" } };
    private TidyToolWindowView tidyToolWindowView;
    public TidyToolWindowViewModel(TidyToolWindowView tidyToolWindowView)
    {
      this.tidyToolWindowView = tidyToolWindowView;
    }

    #region Properties

    public List<FileModel> Files
    {
      get
      {
        return files;
      }
      set
      {
        files = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Files"));
      }
    }
    //public ObservableCollection<SelectedFileModel> SelectedFiles { get; set; } = new ObservableCollection<SelectedFileModel>();

    #endregion
  }
}
