using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class TidyOptions : DialogPage
  {
    #region Properties

    [Category("Tidy")]
    [DisplayName("Tidy Flags")]
    [Description("Array of project(s) to ignore, from the matched ones. If empty, all already matched projects are compiled.")]
    [TypeConverter(typeof(StringArrayConverter))]
    public string[] TidyFlags { get; set; }

    [Category("Tidy")]
    [DisplayName("Fix")]
    [Description("")]
    public bool Fix { get; set; }

    #endregion

  }
}
