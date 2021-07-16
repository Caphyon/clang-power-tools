using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  public class TaskErrorModel : ErrorTask
  {
    #region Properties

    public string FullMessage { get; set; }

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
      return FullMessage.Replace('/', '\\').GetHashCode();
    }

    #endregion

  }
}
