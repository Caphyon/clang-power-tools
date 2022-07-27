namespace ClangPowerToolsShared.Commands
{
  public enum LookInMenu
  {
    EntireSolution,
    CurrentSetProject,
    CurrentActiveDocument,
  }

  public class MenuItem
  {
    public MenuItem(string name, LookInMenu lookInMenu)
    {
      Name = name;
      LookInMenu = lookInMenu;
    }

    public string Name;
    public LookInMenu LookInMenu;
  }
}
