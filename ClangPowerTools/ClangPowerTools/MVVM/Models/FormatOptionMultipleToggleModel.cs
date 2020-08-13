using System.Collections.Generic;
using System.Text;

namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionMultipleToggleModel : FormatOptionModel
  {
    #region Properties

    public List<ToggleModel> ToggleFlags { get; set; } = new List<ToggleModel>();

    #endregion

    #region Constructor

    public FormatOptionMultipleToggleModel()
    {
      HasMultipleToggle = true;
    }

    #endregion

    #region Methods

    public string CreateFlag()
    {
      var sb = new StringBuilder();
      foreach (var item in ToggleFlags)
      {
        sb.AppendLine(string.Concat(item.Name, ": ", item.Value.ToString()));
      }
      return sb.ToString();
    }

    #endregion

  }
}
