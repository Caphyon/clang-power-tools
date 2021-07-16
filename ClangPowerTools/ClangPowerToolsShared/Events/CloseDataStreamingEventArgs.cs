namespace ClangPowerTools.Events
{
  public class CloseDataStreamingEventArgs
  {
    public bool IsStopped { get; set; }

    public CloseDataStreamingEventArgs(bool isStopped)
    {
      IsStopped = isStopped;
    }
  }
}
