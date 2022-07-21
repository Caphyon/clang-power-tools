using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Interfaces;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class DefaultArgsModel : ComponentVisibility, IViewMatcher
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private string functionName = string.Empty;
    private int defaultArgsPosition = 0;

    public DefaultArgsModel() { }

    public string Name { get; } = "Function called with default argument(s)";

    public int Id { get; } = FindCommandIds.kDefaultArgsId;

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

    public string Details { get; } = "Matches invocations of a function where some of the parameters are not explicitly set (default parameters).\n\n" +
    "For example:\nvoid test(int p1 = 10, string p2 = \"a\") { }\n" +
      "test();               // Matched by 1 or 0 explicit arguments\n" +
      "test(20);           // Matched by 1 explicit arguments\n" +
      "test(20, \"Z\");    // Never matched \n";
  }

}
