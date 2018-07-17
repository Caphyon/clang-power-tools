using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Output
{
  public class OutputContent
  {
    #region Properties

    public HashSet<TaskErrorModel> Errors { get; set; } = new HashSet<TaskErrorModel>();
    public List<string> Buffer { get; set; } = new List<string>();
    public string Text { get; set; }
    public bool MissingLLVM { get; set; }


    #endregion

  }
}
