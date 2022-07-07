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

    public string Details { get; } = "Matches invocations of a function where some of the parameters are not explicitly set (default parameters).\n\n" +
    "For example:\nvoid test(int p1 = 10, string p2 = \"a\") { }\n" +
      "test();               // Matched by 1 or 0 explicit arguments\n" +
      "test(20);           // Matched by 1 explicit arguments\n" +
      "test(20, \"Z\");    // Never matched \n";


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
