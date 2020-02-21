namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionToggleModel : FormatOptionModel
  {
    #region Members

    private ToggleValues booleanComboboxValue = ToggleValues.False;

    #endregion

    #region Constructor

    public FormatOptionToggleModel()
    {
      HasBooleanCombobox = true;
    }

    #endregion


    #region Properties

    public ToggleValues BooleanCombobox
    {
      get
      {
        return booleanComboboxValue;
      }
      set
      {
        booleanComboboxValue = value;
        if (IsEnabled == false) 
          IsEnabled = true;

        OnPropertyChanged("BooleanCombobox");
      }
    }

    #endregion
  }
}
