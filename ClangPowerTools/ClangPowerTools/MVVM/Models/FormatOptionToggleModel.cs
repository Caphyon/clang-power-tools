namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionToggleModel : FormatOptionModel
  {
    private ToggleValues booleanComboboxValue = ToggleValues.False;


    public FormatOptionToggleModel()
    {
      HasBooleanCombobox = true;
    }

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
  }
}
