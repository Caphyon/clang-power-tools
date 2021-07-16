namespace ClangPowerTools.MVVM.Models
{
  public class ToggleModel
  {
    #region Properties 

    public string Name { get; set; }
    public ToggleValues Value { get; set; }

    #endregion

    #region Constructor

    public ToggleModel(string name, ToggleValues value)
    {
      Name = name;
      Value = value;
    }

    #endregion
  }
}
