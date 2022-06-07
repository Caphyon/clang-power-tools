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

    public string Details { get; } = "Matches all called expressions with a default argument on a given position.\n" +
    "For example:\nvoid foo(int a = 0, int b = 1) { }\n\nvoid main() {\nfoo();\nfoo(1);\n}" +
      "\nCalledFunction: foo\nDefaultArgPosition: 0\n🔎 Will match foo()";


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
