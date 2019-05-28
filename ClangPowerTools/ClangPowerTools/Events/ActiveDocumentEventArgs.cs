namespace ClangPowerTools.Events
{
  public class ActiveDocumentEventArgs
  {
    public bool IsActiveDocument { get; set; }

    public ActiveDocumentEventArgs(bool isActiveDocument)
    {
      IsActiveDocument = isActiveDocument;
    }
  }
}
