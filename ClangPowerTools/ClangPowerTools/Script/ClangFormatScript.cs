using ClangPowerTools.DialogPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Script
{
  public class ClangFormatScript : ScriptBuiler
  {
    public void ConstructParameters(ClangFormatPage aClangFormatPage)
    {
      mParameters = string.Empty;




      


    }

    #region Get Parameters Helpers

    //Get the clang-format.exe path
    protected override string GetFilePath() => Path.Combine(base.GetFilePath(), ScriptConstants.kClangFormat);




    #endregion
  }
}
