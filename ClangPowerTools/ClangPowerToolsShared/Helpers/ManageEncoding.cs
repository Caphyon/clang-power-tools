using System.IO;
using System.Text;

namespace ClangPowerToolsShared.Helpers
{
  public class ManageEncoding
  {
    //change encoding from utf8 BOM to utf8
    public static void ChangeEncodingFromBomToUtf8(string inputFile, string outputFile)
    {
      using (StreamReader reader = new StreamReader(inputFile, Encoding.UTF8, true))
      {
        string content = reader.ReadToEnd();
        using (StreamWriter writer = new StreamWriter(outputFile, false, new UTF8Encoding(false)))
        {
          writer.Write(content);
        }
      }
    }
  }
}
