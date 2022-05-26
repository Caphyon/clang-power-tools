namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class DefaultArgsModel
  {
    private string functionName = string.Empty;
    private int defaultArgsPosition = 0;

    public string FunctionName
    {
      get { return functionName; }
      set
      {
        functionName = value;
      }
    }
  }
}
