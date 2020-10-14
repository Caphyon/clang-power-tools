using ClangPowerTools.Extensions;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;

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

    public static List<IFormatOption> GetDefaultOptionsForStyle(EditorStyles style)
    {
      switch (style)
      {
        case EditorStyles.Custom:
          break;
        case EditorStyles.LLVM:
          return new FormatOptionsData().FormatOptions;
        case EditorStyles.Google:
          return new FormatOptionsGoogleData().FormatOptions;
        case EditorStyles.Chromium:
          return new FormatOptionsChromiumData().FormatOptions;
        case EditorStyles.Microsoft:
          return new FormatOptionsMicrosoftData().FormatOptions;
        case EditorStyles.Mozilla:
          return new FormatOptionsMozillaData().FormatOptions;
        case EditorStyles.WebKit:
          return new FormatOptionsWebKitData().FormatOptions;
        default:
          break;
      }
      return new FormatOptionsData().FormatOptions;
    }

    public static List<IFormatOption> CloneDetectedOptions(List<IFormatOption> formatOptions)
    {
      var clonedOptions = new List<IFormatOption>();
      foreach (var option in formatOptions)
      {
        switch (option)
        {
          case FormatOptionToggleModel toggleModel:
            clonedOptions.Add(toggleModel.Clone());
            break;
          case FormatOptionInputModel inputModel:
            clonedOptions.Add(inputModel.Clone());
            break;
          case FormatOptionMultipleToggleModel multipleToggleModel:
            clonedOptions.Add(multipleToggleModel.Clone());
            break;
          case FormatOptionMultipleInputModel multipleInputModel:
            clonedOptions.Add(multipleInputModel.Clone());
            break;
          default:
            break;
        }
      }
      return clonedOptions;
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
