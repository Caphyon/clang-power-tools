using EnvDTE;

namespace ClangPowerTools
{
  class CurrentSolution : IItem
  {
    #region Members

    private readonly Solution solution;
    private readonly string saveAsPath = string.Empty;

    #endregion

    #region Constructor

    public CurrentSolution(Solution solution) => this.solution = solution;

    public CurrentSolution(Solution solution, string savePath)
    {
      this.solution = solution;
      saveAsPath = savePath;
    }

    #endregion

    public string GetName() => solution.FileName;

    public object GetObject() => solution;

    public string GetPath() => solution.FullName;

    public void Save() => solution.SaveAs(saveAsPath);
  }
}
