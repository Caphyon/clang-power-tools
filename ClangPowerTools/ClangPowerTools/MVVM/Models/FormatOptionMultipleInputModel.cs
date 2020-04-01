namespace ClangPowerTools.MVVM.Models
{
  class FormatOptionMultipleInputModel : FormatOptionModel
  {
    #region Members

    private string input = string.Empty;

    #endregion

    #region Constructor

    public FormatOptionMultipleInputModel()
    {
      HasMultipleInputTextBox = true;
    }

    #endregion

    #region Properties

    public string MultipleInput
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

        OnPropertyChanged("MultipleInput");
      }
    }

    #endregion
  }
}
