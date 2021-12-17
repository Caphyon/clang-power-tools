using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.IgnoreActions
{
  /// <summary>
  /// Logic for all the ignore actions types on any abject
  /// </summary>
  public class IgnoreItem
  {
    #region Members

    /// <summary>
    /// Message to display if the checked file 
    /// is on the ignore list after a clang compile/tidy command
    /// </summary>
    private string ignoreCompileOrTidyMessage;

    /// <summary>
    /// Message to display if the checked file 
    /// is on the ignore list after a clang-format command
    /// </summary>
    private string ignoreFormatMessage;

    #endregion

    #region Properties

    /// <summary>
    /// Message for the ignored files for clang compile/tidy
    /// The value is null if the proper check wasn't executed before.
    /// </summary>
    public string IgnoreCompileOrTidyMessage
    {
      get
      {
        return ignoreCompileOrTidyMessage;
      }
      private set
      {
        ignoreCompileOrTidyMessage =
          $"Cannot use clang compile/tidy on ignored files.\nTo enable clang compile/tidy remove the {value} from Clang Power Tools Settings -> Compiler -> Files/Projects to ignore.";
      }
    }

    /// <summary>
    /// Message for the ignored files for clang-format
    /// The value is null if the proper check wasn't executed before.
    /// </summary>
    public string IgnoreFormatMessage
    {
      get
      {
        return ignoreFormatMessage;
      }
      private set
      {
        ignoreFormatMessage =
          $"\nCannot use clang-format on ignored files.\nTo enable clang-format remove the \"{value}\" file from Clang Power Tools Settings -> Format -> Files to ignore.";
      }
    }

    #endregion


    #region Methods

    /// <summary>
    /// Check if the given parameter 
    /// is on the clang compile/tidy ignore list
    /// </summary>
    /// <param name="checkedItem">Object to be checked</param>
    /// <returns>True if the given parameter is on the clang compile/tidy ignore list. False otherwise</returns>
    public bool Check(IItem checkedItem, List<string> paths = null)
    {
      if (checkedItem is CurrentProjectItem)
      {
        ProjectItem projectItem = checkedItem.GetObject() as ProjectItem;
        IgnoreCompileOrTidyMessage = $"\"{projectItem.Name}\" file";

        return SettingsProvider.CompilerSettingsModel.FilesToIgnore.Contains(projectItem.Name);
      }
      else if (checkedItem is CurrentProject)
      {
        Project project = checkedItem.GetObject() as Project;
        IgnoreCompileOrTidyMessage = $"\"{project.Name}\" project";

        return SettingsProvider.CompilerSettingsModel.ProjectsToIgnore.Contains(project.Name);
      }


      return false;
    }


    /// <summary>
    /// Check if the given parameter 
    /// is on the clang compile/tidy ignore list
    /// </summary>
    /// <param name="checkedItem">Object to be checked</param>
    /// <returns>True if the given parameter is on the clang-format ignore list. False otherwise</returns>
    public bool Check(Document checkedItem)
    {
      var skipFilesList = SettingsProvider.FormatSettingsModel.FilesToIgnore
        .ToLower()
        .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

      IgnoreFormatMessage = checkedItem.Name;

      return skipFilesList
        .Contains(checkedItem.Name.ToLower());
    }

    #endregion

  }
}
