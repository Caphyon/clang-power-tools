namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionInputModel : FormatOptionModel
  {
    private string input = string.Empty;

    public FormatOptionInputModel()
    {
      HasInputTextBox = true;
    }

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
  }
}
