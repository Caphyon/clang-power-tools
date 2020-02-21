namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionInputModel : FormatOptionModel
  {
    #region Members

    private string input = string.Empty;

    #endregion

    #region Constructor

    public FormatOptionInputModel()
    {
      HasInputTextBox = true;
    }

    #endregion

    #region Properties

    public string Input
    {
      get
      {
        return input;
      }
      set
      {
        input = value;
        if (IsEnabled == false) 
          IsEnabled = true;

        OnPropertyChanged("Input");
      }
    }

    #endregion
  }
}
