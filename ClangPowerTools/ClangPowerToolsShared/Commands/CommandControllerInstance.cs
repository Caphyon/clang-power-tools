namespace ClangPowerTools
{
  public class CommandControllerInstance
  {
    public static CommandControllerInstance Instance { get { return instance; } }
    public static CommandController CommandController { get; set; }
    public static readonly CommandControllerInstance instance = new();
  }
}
