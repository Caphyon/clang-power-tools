using System.Collections.Generic;

namespace ClangPowerTools.Output
{
  public class OutputContentModel
  {
    #region Properties

    public HashSet<TaskErrorModel> Errors { get; set; } = new HashSet<TaskErrorModel>();
    public List<string> Buffer { get; set; } = new List<string>();
    public string Text { get; set; }
    public bool MissingLLVM { get; set; }
    public bool HasEncodingError { get; set; }
    public string JsonFilePath { get; set; }


    #endregion

  }
}
