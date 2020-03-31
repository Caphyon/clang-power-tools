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

    public static FormatOptionsData CustomOptionsData = new FormatOptionsData();
    public static FormatOptionsData LlvmOptionsData = new FormatOptionsData();
    public static FormatOptionsGoogleData GoogleOptionsData = new FormatOptionsGoogleData();
    public static FormatOptionsChromiumData ChromiumOptionsData = new FormatOptionsChromiumData();
    public static FormatOptionsMozillaData MozillaOptionsData = new FormatOptionsMozillaData();
    public static FormatOptionsWebKitData WebkitOptionsData = new FormatOptionsWebKitData();
    public static FormatOptionsMicrosoftData MicrosoftOptionsData = new FormatOptionsMicrosoftData();

    #endregion

    #region Methods
    public static void ResetOptions()
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
