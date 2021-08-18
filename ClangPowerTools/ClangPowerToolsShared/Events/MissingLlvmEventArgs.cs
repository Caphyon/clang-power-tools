namespace ClangPowerTools.Events
{
  public class MissingLlvmEventArgs
  {
    public bool MissingLLVM { get; set; }

    public MissingLlvmEventArgs(bool aIsMissing)
    {
      MissingLLVM = aIsMissing;
    }
  }
}
