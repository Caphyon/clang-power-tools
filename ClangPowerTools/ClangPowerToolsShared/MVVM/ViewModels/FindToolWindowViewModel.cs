using ClangPowerTools;
using ClangPowerTools.Views;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.ViewModels
{

  public class FindToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private FindToolWindowView findToolWindowView;

    public FindToolWindowViewModel(FindToolWindowView findToolWindowView)
    {
      this.findToolWindowView = findToolWindowView;
    }
  }
}
