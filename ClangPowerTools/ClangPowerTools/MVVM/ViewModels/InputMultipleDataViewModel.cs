namespace ClangPowerTools
{
  public class InputMultipleDataViewModel
  {
    #region Properties

    public string Input { get; set; }

    #endregion

    #region Constructor 

    public InputMultipleDataViewModel(string input)
    {
      Input = input;
    }

    // Empty constructor for XAML intelisense
    public InputMultipleDataViewModel()
    {

    }

    #endregion
  }
}
