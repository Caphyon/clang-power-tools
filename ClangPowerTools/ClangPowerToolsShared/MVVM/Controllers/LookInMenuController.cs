using System.Collections.Generic;

namespace ClangPowerToolsShared.Commands
{
  public static class LookInMenuController
  {
    private static MenuItem selectedMenuItem = new();

    public static void SetSelectedOption(MenuItem menuItem)
    {
      selectedMenuItem = menuItem;
    }

    public static MenuItem GetSelectedMenuItem()
    {
      return selectedMenuItem;
    }

    public static List<MenuItem> MenuOptions
    {
      get
      {
        return new List<MenuItem>()
        {
          new MenuItem ("Entire solution", LookInMenu.EntireSolution),
          new MenuItem ("Current project", LookInMenu.CurrentProject),
          new MenuItem ("Current active document", LookInMenu.CurrentActiveDocument),
        };
      }
    }
  }
}
