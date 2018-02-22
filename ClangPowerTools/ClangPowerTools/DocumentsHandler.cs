using EnvDTE;

namespace ClangPowerTools
{
  public class DocumentsHandler
  {
    public static Documents GetActiveDocuments(DTE aDte) => aDte.Documents;

    public static Document GetActiveDocument(DTE aDte) => aDte.ActiveDocument;

    public static void SaveActiveDocuments(DTE aDte) => GetActiveDocuments(aDte).SaveAll();

    public static void SaveActiveDocument(DTE aDte) => GetActiveDocument(aDte).Save();
  }
}
