using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class DefaultArgsModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private string functionName = string.Empty;
    private int defaultArgsPosition = 0;

    public string FunctionName
    {
      get { return functionName; }
      set
      {
        functionName = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FunctionName"));
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
  }
}
