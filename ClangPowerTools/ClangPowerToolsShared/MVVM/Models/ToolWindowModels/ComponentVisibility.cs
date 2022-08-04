using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class ComponentVisibility : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private string visibility = string.Empty;

    public ComponentVisibility()
    {
      visibility = UIElementsConstants.Hidden;
    }

    public string Visibility
    {
      get { return visibility; }
    }

    public void Hide()
    {
      visibility = UIElementsConstants.Hidden;
    }

    public void Show()
    {
      visibility = UIElementsConstants.Visibile;
    }
  }
}
