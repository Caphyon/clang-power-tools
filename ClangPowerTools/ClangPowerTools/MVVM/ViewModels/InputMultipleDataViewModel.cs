namespace ClangPowerTools
{
  public class InputMultipleDataViewModel
  {
    #region Members;

    private string input;

    #endregion


    #region Constructor 

    public InputMultipleDataViewModel(string input)
    {
      this.input = input;
    }

    #endregion

    #region Properties

    public string Input
    {
      get => input;
      set => input = value;
    }

    #endregion
  }
}
