using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using System.Collections.Generic;

namespace ClangPowerToolsShared.MVVM.Controllers
{
  public class FindController
  {
    private int currentCommand = 0;
    
    public void LaunchCommand(int commandId, List<string> paths)
    {
      currentCommand = commandId;
      switch (currentCommand)
      {
        case FindCommandIds.kDefaultArgs:
        {
          List<string> commands = new();
          string result = MatchConstants.GetListPowershell(paths);
          break;
        }
        default:
          break;
      }
    }
  }
}
