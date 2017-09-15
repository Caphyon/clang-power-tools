using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class GeneralOptions : DialogPage
  {
    [Category("General")]
    [DisplayName("Project to ignore")]
    [Description("Array of project(s) to ignore, from the matched ones. If empty, all already matched projects are compiled.")]
    [TypeConverter(typeof(StringArrayConverter))]
    public string[] ProjectsToIgnore { get; set; }

    [Category("General")]
    [DisplayName("Include Directories")]
    [Description("Directories to be used for includes (libraries, helpers).")]
    [TypeConverter(typeof(StringArrayConverter))]
    public string[] IncludeDirectories { get; set; }

    [Category("General")]
    [DisplayName("Continue On Error")]
    [Description("Switch to continue project compilation even when errors occur.")]
    public bool Continue { get; set; }

    [Category("General")]
    [DisplayName("Compile Flags")]
    [Description("Flags given to clang++ when compiling project, alongside project - specific defines.")]
    [TypeConverter(typeof(StringArrayConverter))]
    public string[] ClangFlags { get; set; }

  }
}
