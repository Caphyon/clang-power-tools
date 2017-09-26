using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class GeneralOptions : DialogPage
  {
    #region Members

    private string[] mClangFlags = new string[] { };

    #endregion

    #region Properties

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
    [Description("Flags given to clang++ when compiling project, alongside project - specific defines. If empty the default flags will be loaded.")]
    [TypeConverter(typeof(StringArrayConverter))]
    public string[] ClangFlags
    {
      get => 0 == mClangFlags.Length ? DefaultOptions.kClangFlags : mClangFlags;
      set => mClangFlags = value;
    }

    #endregion

  }
}
