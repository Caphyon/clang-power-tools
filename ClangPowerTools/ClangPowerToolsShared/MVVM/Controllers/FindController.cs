using ClangPowerToolsShared.Commands;

namespace ClangPowerToolsShared.MVVM.Controllers
{
  public class FindController
  {
    private int currentCommand = 0;
    
    public void LaunchCommand(int commandId)
    {
      currentCommand = commandId;
      switch (currentCommand)
      {
        case FindCommandIds.kDefaultArgs:
        {
          break;
        }
        default:
          break;
      }
    }
  }
}
