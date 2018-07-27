using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools
{
  public class TaskErrorModel
  {
    #region Properties

    public string FilePath { get; set; }

    public int Line { get; set; }

    public int Column { get; set; }

    public TaskErrorCategory Category { get; set; }

    public string FullMessage { get; set; }

    public string Description { get; set; }

    public IVsHierarchy HierarchyItem { get; set; }

    #endregion


    #region Public Methods

    public override bool Equals(object obj)
    {
      var otherObj = obj as TaskErrorModel;
      if (null == otherObj)
        return false;

      return FullMessage.Replace('/', '\\') == otherObj.FullMessage.Replace('/', '\\');
    }

    public override int GetHashCode()
    {
      return FullMessage.GetHashCode();
    }

    #endregion

  }
}
