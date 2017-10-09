using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class ErrorCreator
  {
    #region Members

    private const string kCompileErrorsRegex = @"(.\:\\[ \w+\\\/.]*[h|cpp])(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*(\d+)(\r\n|\r|\n| |:)*error(\r\n|\r|\n| |:)*(.*)";
    private int kBufferSize = 5;
    private int countLines;
    private List<string> mOutputBuffer = new List<string>();

    #endregion

    #region Constructor



    #endregion



  }
}
