namespace ClangPowerToolsShared.Commands
{
  public enum LookInMenu
  {
    EntireSolution,
    CurrentProject,
    CurrentActiveDocument,
  }

  public class MenuItem
  {
    public MenuItem() { }
    public MenuItem(string name, LookInMenu lookInMenu)
    {
      Name = name;
      LookInMenu = lookInMenu;
    }

    public string Name { get; set; }
    public LookInMenu LookInMenu { get; set; }
  }
}
