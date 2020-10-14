using System.IO;
using System.Text;

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
        var editedInput = new StringBuilder();
        using (var reader = new StringReader(input))
        {
          string line;
          while ((line = reader.ReadLine()) != null)
          {
            editedInput.AppendLine(line.TrimStart());
          }
        }
        return editedInput.ToString();
      }
      set
      {
        input = value;
        if (IsEnabled == false)
          IsEnabled = true;

        var editedInput = new StringBuilder();
        using (var reader = new StringReader(input))
        {
          string line;
          while ((line = reader.ReadLine()) != null)
          {
            editedInput.Append("  ");
            editedInput.AppendLine(line);
          }
        }
        input = editedInput.ToString();

        OnPropertyChanged("MultipleInput");
      }
    }

    #endregion
  }
}
