using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class FormatOptionsProvider
  {
    #region Members

    public static FormatOptionsData CustomOptionsData;
    public static FormatOptionsData LlvmOptionsData;
    public static FormatOptionsGoogleData GoogleOptionsData;
    public static FormatOptionsChromiumData ChromiumOptionsData;
    public static FormatOptionsMozillaData MozillaOptionsData;
    public static FormatOptionsWebKitData WebkitOptionsData;
    public static FormatOptionsMicrosoftData MicrosoftOptionsData;

    #endregion

    #region Constructor

    static FormatOptionsProvider()
    {
      InitializeFormatData();
    }

    #endregion

    #region Methods
    public static void ResetOptions()
    {
      InitializeFormatData();
    }

    private static void InitializeFormatData()
    {
      CustomOptionsData = new FormatOptionsData();
      LlvmOptionsData = new FormatOptionsData();
      GoogleOptionsData = new FormatOptionsGoogleData();
      ChromiumOptionsData = new FormatOptionsChromiumData();
      MozillaOptionsData = new FormatOptionsMozillaData();
      WebkitOptionsData = new FormatOptionsWebKitData();
      MicrosoftOptionsData = new FormatOptionsMicrosoftData();
    }

    #endregion

  }
}
