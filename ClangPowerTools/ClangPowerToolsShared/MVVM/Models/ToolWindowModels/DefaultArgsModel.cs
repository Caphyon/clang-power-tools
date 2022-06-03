using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class DefaultArgsModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private string functionName = string.Empty;
    private int defaultArgsPosition = 0;
    private string visibility = string.Empty;

    public DefaultArgsModel()
    {
      visibility = UIElementsConstants.Hidden;
    }

    public string FunctionName
    {
      get { return functionName; }
      set
      {
        functionName = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FunctionName"));
      }
    }

    public string Visibility
    {
      get { return visibility; }
      set
      {
        visibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
      }
    }

    public int DefaultArgsPosition
    {
      get { return defaultArgsPosition; }
      set
      {
        defaultArgsPosition = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DefaultArgsPosition"));
      }
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
