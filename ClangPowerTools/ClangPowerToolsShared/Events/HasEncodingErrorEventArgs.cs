using ClangPowerTools.Output;

namespace ClangPowerTools.Events
{
  public class HasEncodingErrorEventArgs
  {
    public OutputContentModel Model { get; set; }

    public HasEncodingErrorEventArgs(OutputContentModel model)
    {
      Model = model;
    }
  }
}
