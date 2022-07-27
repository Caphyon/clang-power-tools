using System.Collections.Generic;

namespace ClangPowerToolsShared.Commands
{
  public static class LookInMenuController
  {
    public static List<MenuItem> GetSelected()
    {
      return new List<MenuItem>()
      {
        new MenuItem ("Entire solution", LookInMenu.EntireSolution),
        new MenuItem ("Current project", LookInMenu.CurrentSetProject),
        new MenuItem ("Current active document", LookInMenu.CurrentActiveDocument),
      };
    }
  }
}
